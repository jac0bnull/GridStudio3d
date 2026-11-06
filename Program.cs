using GridStudio;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
var opts=new WebApplicationOptions{Args=args,ContentRootPath=AppContext.BaseDirectory,WebRootPath=Path.Combine(AppContext.BaseDirectory,"wwwroot")};
var builder=WebApplication.CreateBuilder(opts);
var port=builder.Configuration.GetValue<int>("port",0);
builder.WebHost.UseKestrel(k=>{k.Listen(IPAddress.Loopback,port);k.Limits.MaxRequestBodySize=2500000;});
var app=builder.Build();var token=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
var root=builder.Configuration["library"]??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GridStudio");
var preferences=new WorkspacePreferences(root, builder.Configuration["library"]==null);
var cache=new AssetCacheManager(root,preferences);using var http=new HttpClient(new HttpClientHandler{AllowAutoRedirect=false,AutomaticDecompression=DecompressionMethods.All}){Timeout=TimeSpan.FromSeconds(90)};
http.DefaultRequestHeaders.UserAgent.ParseAdd("GRID-Studio/0.2.1-alpha");
using var lookup = new MarketLookup(http);
using var gallery=new IcosaDiscovery(http);var provider=new PolyHaven(http);var search=new AssetSearchService(provider);var meta=new AssetMetadataService(provider);var downloads=new AssetDownloadManager(http,cache,meta);
app.Use(async(ctx,next)=>{
    if(ctx.Request.Host.Host!="127.0.0.1"){ctx.Response.StatusCode=403;return;}
    if(ctx.Request.Method!="GET"&&ctx.Request.Method!="HEAD"&&ctx.Request.Headers["X-Grid-Session"]!=token){ctx.Response.StatusCode=403;return;}
    ctx.Response.Headers["X-Content-Type-Options"]="nosniff";ctx.Response.Headers["Referrer-Policy"]="no-referrer";ctx.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; worker-src 'self' blob:; img-src 'self' blob: data: https://cdn.polyhaven.com https://cdn.shopify.com https://s3.us-east-005.backblazeb2.com; connect-src 'self' blob: data:; style-src 'self' 'unsafe-inline'; object-src 'none'; frame-ancestors 'none'; base-uri 'none'";
    try{await next();}catch(OperationCanceledException)when(ctx.RequestAborted.IsCancellationRequested){}catch(Exception e){if(!ctx.Response.HasStarted){ctx.Response.StatusCode=e is InvalidDataException or JsonException?400:502;await ctx.Response.WriteAsJsonAsync(new{error=e.Message});}}
});
app.UseDefaultFiles();app.UseStaticFiles();
object UserSettings(){var s=preferences.State;return new{theme=s.Theme,downloadLocation=s.Active,defaultLocation=preferences.DefaultLocation};}
app.MapPost("/api/settings/folder",async()=>Results.Json(new{path=await FolderChoice.Pick()}));
app.MapGet("/api/settings",()=>Results.Json(UserSettings()));
app.MapPost("/api/settings",(PreferenceEdit edit)=>{preferences.Update(edit);return Results.Json(UserSettings());});
app.MapGet("/api/session",()=>Results.Json(new{token,version="0.2.1-alpha"}));
app.MapGet("/api/assets",async(CancellationToken ct)=>await search.All(ct));
app.MapGet("/api/discover/model/{id}",async(string id,CancellationToken ct)=>(await gallery.Resolve(id,ct)).Asset);
app.MapGet("/api/search/term", async (string q, CancellationToken ct) => await lookup.Translate(q, ct));
app.MapGet("/api/search/offers", async (string q, CancellationToken ct) => await lookup.Paid(q, ct));
app.MapGet("/api/discover",async(string q,CancellationToken ct)=>await gallery.Search(q,ct));
app.MapGet("/api/files/{id}",async(string id,CancellationToken ct)=>Results.Json(id.StartsWith("icosa_",StringComparison.Ordinal)?(await gallery.Resolve(id,ct)).Files:await meta.Resolve(id,ct)));
app.MapGet("/api/installed",()=>cache.List().ToArray());
app.MapGet("/api/doc/{key}",(string key)=>key is "scene" or "hud" or "favorites" && File.Exists(Path.Combine(cache.Documents,key+".json"))?Results.File(Path.Combine(cache.Documents,key+".json"),"application/json"):Results.NotFound());
app.MapPost("/api/doc/{key}",(string key,JsonElement data)=>{cache.StoreDocument(key,data);return Results.NoContent();});
app.MapGet("/library/{id}/{**file}",(string id,string file)=>{var r=cache.Installed(id);return r!=null&&r.Sha256.ContainsKey(Guard.PathPart(file))?Results.File(Path.Combine(cache.Dir(id),file),Path.GetExtension(file).ToLowerInvariant() switch {".gltf"=>"model/gltf+json",".png"=>"image/png",".jpg" or ".jpeg"=>"image/jpeg",_=>"application/octet-stream"}):Results.NotFound();});
app.MapPost("/api/install/{id}",async(string id,HttpContext ctx)=>{
    var ct=ctx.RequestAborted;
    ctx.Response.ContentType="application/x-ndjson";async Task Line(object v){await ctx.Response.WriteAsync(JsonSerializer.Serialize(v,JsonSerializerOptions.Web)+"\n",ct);await ctx.Response.Body.FlushAsync(ct);}
    try{Receipt result;
        if(id.StartsWith("icosa_",StringComparison.Ordinal)){var item=await gallery.Resolve(id,ct);result=await downloads.DownloadGallery(item.Asset,item.Files,(done,total)=>Line(new{done,total}),ct);}
        else {var asset=(await search.All(ct)).FirstOrDefault(x=>x.Id==id)??throw new InvalidDataException("Unknown provider asset");result=await downloads.Download(asset,(done,total)=>Line(new{done,total}),ct);}await Line(new{receipt=result});}catch(Exception e)when(!ct.IsCancellationRequested){await Line(new{error=e.Message});}
});
app.MapPost("/api/local/{name}",(HttpContext ctx,string name)=>LocalModels.Import(ctx,name,cache));
app.MapPost("/api/export/{name}",async(HttpContext ctx,string name)=>{
    if(name is not ("grid-hud-unity.zip" or "grid-studio-unity.zip"))return Results.BadRequest();
    var feature=ctx.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>();if(feature!=null&&!feature.IsReadOnly)feature.MaxRequestBodySize=512L*1024*1024;
    var dir=Directory.CreateDirectory(Path.Combine(root,"exports")).FullName;
    var filename=Path.GetFileNameWithoutExtension(name)+"-"+Guid.NewGuid().ToString("N")+".zip";var path=Path.Combine(dir,filename);var tmp=path+".partial";
    try{await using(var f=new FileStream(tmp,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)){await ctx.Request.Body.CopyToAsync(f,ctx.RequestAborted);f.Position=0;using var zip=new System.IO.Compression.ZipArchive(f,System.IO.Compression.ZipArchiveMode.Read,true);if(!zip.Entries.Any(e=>e.FullName=="Assets/GridStudioExport/Resources/GridHudLayout.json"))throw new InvalidDataException("Missing HUD layout in export");}
        File.Move(tmp,path);return Results.Json(new{filename,path});}finally{if(File.Exists(tmp))File.Delete(tmp);}
});
await app.StartAsync();var url=app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
Console.WriteLine($"GRID STUDIO: {url} (close this window to stop)");
if(!args.Contains("--no-browser"))Process.Start(new ProcessStartInfo(url){UseShellExecute=true});
await app.WaitForShutdownAsync();









$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Collections -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
public static class SettingFill {
 public static void Fill(string input,string output,string asset,string polygon,string areas){
  Bitmap src=new Bitmap(input);int w=src.Width,h=src.Height;
  Bitmap dst=new Bitmap(w,h,PixelFormat.Format32bppArgb);
  var points=new List<PointF>();foreach(string pair in polygon.Split(';')){var v=pair.Split(',');points.Add(new PointF(float.Parse(v[0]),float.Parse(v[1])));}
  var rects=new List<Rectangle>();foreach(string area in areas.Split(';')){if(area=="")continue;var a=area.Split(',');rects.Add(new Rectangle(int.Parse(a[0]),int.Parse(a[1]),int.Parse(a[2]),int.Parse(a[3])));}
  for(int y=0;y<h;y++)for(int x=0;x<w;x++){
   bool inside=false;for(int a=0,b=points.Count-1;a<points.Count;b=a++)if(((points[a].Y>y)!=(points[b].Y>y))&&(x<(points[b].X-points[a].X)*(y-points[a].Y)/(points[b].Y-points[a].Y)+points[a].X))inside=!inside;
   if(!inside)continue;
   Color c=src.GetPixel(x,y);bool replace=c.A==0;
   foreach(var rect in rects)if(rect.Contains(x,y)&&x>8&&y>8&&x<w-9&&y<h-9)replace=true;
   if(replace){
    Color fill=Color.FromArgb(99,87,73);int sample=-1;
    if(asset=="panel_background")sample=y<145?120:220;
    if(asset=="version_panel")sample=950;
    if(asset=="patch_button")sample=490;
    if(asset=="delete_button")sample=800;
    if(asset=="delete_frame")sample=15;
    if(asset.EndsWith("dropdown"))sample=130;
    if(asset.EndsWith("toggle"))sample=180;
    if(asset=="autosave_on_fill")sample=110;
    if(asset=="levelup_off_fill")sample=116;
    if(asset=="close_button")sample=23;
    if(sample>=0&&sample<w){var s=src.GetPixel(sample,y);if(s.A>0)fill=Color.FromArgb(s.R,s.G,s.B);}
    // Restore continuous empty bases underneath foreground components.
    if(asset=="delete_frame"&&x>25&&x<w-25&&y>25&&y<h-25)fill=Color.FromArgb(78,59,48);
    if(asset.EndsWith("toggle"))fill=Color.FromArgb(61,44,34);
    if(asset=="autosave_on_fill"&&y>10&&y<h-10)fill=Color.FromArgb(194,240,0);
    if(asset=="levelup_off_fill"&&y>8&&y<h-8)fill=Color.FromArgb(255,54,36);
    if(asset=="panel_background"&&y<145)fill=Color.FromArgb(99,87,73);
    if(asset=="close_button")fill=Color.FromArgb(124,110,88);
    c=fill;
   }
   if(asset.EndsWith("dropdown")&&x>8&&x<w-9&&y>8&&y<h-9)c=Color.FromArgb(153,141,116);
   if(asset=="close_button"&&(x-w/2)*(x-w/2)+(y-h/2)*(y-h/2)<40*40)c=Color.FromArgb(124,110,88);
   dst.SetPixel(x,y,Color.FromArgb(255,c.R,c.G,c.B));
  }
  dst.Save(output,ImageFormat.Png);src.Dispose();dst.Dispose();
 }
}
'@
$inputRoot='D:\Download\ExtractedUI\Setting'
$outputRoot=Join-Path $PSScriptRoot '../Temp/SettingFilled'
$backupRoot=Join-Path $PSScriptRoot '../Temp/SettingBeforeFill'
New-Item -ItemType Directory -Force $outputRoot,$backupRoot | Out-Null
$lines=Get-Content (Join-Path $PSScriptRoot 'extract_ui_assets.ps1')
$start=[array]::IndexOf($lines,('$setting=@'+"'"));$end=$start+1
while($lines[$end] -ne "'@"){ $end++ }
$specs=@{}
foreach($line in $lines[($start+1)..($end-1)]){$v=$line.Split('|');$specs[$v[0]]=$v}
$names=@('panel_background','version_panel','patch_button','delete_frame','delete_button','effect_dropdown','sell_dropdown','language_dropdown','autosave_toggle','autosave_on_fill','levelup_toggle','levelup_off_fill','close_button')
foreach($name in $names){
 $v=$specs[$name];$r=$v[1].Split(',');$ox=[int]$r[0];$oy=[int]$r[1]
 $poly=($v[4].Split(';') | ForEach-Object {$p=$_.Split(',');"$([int]$p[0]-$ox),$([int]$p[1]-$oy)"}) -join ';'
 $regions=[System.Collections.Generic.List[string]]::new()
 foreach($child in $specs.Values){if($child[3] -eq $name){$cr=$child[1].Split(',');$regions.Add("$([int]$cr[0]-$ox-3),$([int]$cr[1]-$oy-3),$([int]$cr[2]+6),$([int]$cr[3]+6)")}}
 Copy-Item -LiteralPath (Join-Path $inputRoot "$name.png") -Destination (Join-Path $backupRoot "$name.png")
 [SettingFill]::Fill((Join-Path $inputRoot "$name.png"),(Join-Path $outputRoot "$name.png"),$name,$poly,($regions -join ';'))
 Write-Output "Filled: $name.png"
}

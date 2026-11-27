param([string]$OutputDirectory)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Collections,System.Private.Windows.GdiPlus,System.Private.Windows.Core,System.Console -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
public static class FillCalendar {
 public static void Run(string input,string output,string shape,string regions,string reference) {
  using(var src=new Bitmap(input)) {
   int w=src.Width,h=src.Height,n=w*h;
   var valid=new bool[n];var hole=new bool[n];var known=new bool[n];var r=new float[n];var g=new float[n];var b=new float[n];
   string[] pairs=shape.Split(';');var pts=new PointF[pairs.Length];for(int k=0;k<pairs.Length;k++){var p=pairs[k].Split(',');pts[k]=new PointF(float.Parse(p[0]),float.Parse(p[1]));}
   var rects=new List<Rectangle>();foreach(var s in regions.Split(';'))if(s!=""){var a=s.Split(',');rects.Add(new Rectangle(int.Parse(a[0]),int.Parse(a[1]),int.Parse(a[2]),int.Parse(a[3])));}
   Bitmap donor=reference==""?null:new Bitmap(reference);
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){
    int i=y*w+x;bool inside=false;for(int a=0,c=pts.Length-1;a<pts.Length;c=a++)if(((pts[a].Y>y)!=(pts[c].Y>y))&&(x<(pts[c].X-pts[a].X)*(y-pts[a].Y)/(pts[c].Y-pts[a].Y)+pts[a].X))inside=!inside;
    valid[i]=inside;if(!inside)continue;var color=src.GetPixel(x,y);
    if(donor!=null&&x<140&&y<170){int dx=(int)((double)x/(w-1)*(donor.Width-1)),dy=(int)((double)y/(h-1)*(donor.Height-1));color=donor.GetPixel(dx,dy);}
    bool remove=color.A==0;foreach(var rect in rects)if(rect.Contains(x,y)&&x>9&&y>9&&x<w-10&&y<h-10)remove=true;
    hole[i]=remove;known[i]=!remove;r[i]=color.R;g[i]=color.G;b[i]=color.B;
   }
   if(donor!=null)donor.Dispose();
   var queue=new Queue<int>();var queued=new bool[n];
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*w+x;if(!hole[i])continue;if((x>0&&known[i-1])||(x<w-1&&known[i+1])||(y>0&&known[i-w])||(y<h-1&&known[i+w])){queue.Enqueue(i);queued[i]=true;}}
   int[] offsets={-1,1,-w,w};
   while(queue.Count>0){int i=queue.Dequeue(),x=i%w,y=i/w;float rr=0,gg=0,bb=0;int count=0;
    foreach(int d in offsets){int j=i+d;if(j<0||j>=n||(d==-1&&x==0)||(d==1&&x==w-1))continue;if(known[j]){rr+=r[j];gg+=g[j];bb+=b[j];count++;}}
    if(count==0)continue;r[i]=rr/count;g[i]=gg/count;b[i]=bb/count;known[i]=true;
    foreach(int d in offsets){int j=i+d;if(j<0||j>=n||(d==-1&&x==0)||(d==1&&x==w-1))continue;if(hole[j]&&!queued[j]){queue.Enqueue(j);queued[j]=true;}}
   }
   var holes=new List<int>();for(int i=0;i<n;i++)if(hole[i])holes.Add(i);
   for(int pass=0;pass<160;pass++)foreach(int i in holes){int x=i%w;float rr=0,gg=0,bb=0;int count=0;foreach(int d in offsets){int j=i+d;if(j<0||j>=n||(d==-1&&x==0)||(d==1&&x==w-1)||!valid[j])continue;rr+=r[j];gg+=g[j];bb+=b[j];count++;}if(count>0){r[i]=rr/count;g[i]=gg/count;b[i]=bb/count;}}
   string asset=System.IO.Path.GetFileNameWithoutExtension(input);
   Bitmap frame=asset.StartsWith("day_")?new Bitmap(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(input),"day_4_card.png")):null;
   // Complete the obscured flat-colour UI surfaces without bleeding text colours.
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){
    int i=y*w+x;if(!valid[i])continue;
    if(frame!=null){
     var c=frame.GetPixel((int)((double)x/(w-1)*(frame.Width-1)),(int)((double)y/(h-1)*(frame.Height-1)));
     if(x>=9&&x<w-10&&y>=9&&y<h-10){r[i]=181;g[i]=169;b[i]=147;}
     else if(c.A>0){r[i]=c.R;g[i]=c.G;b[i]=c.B;}
     else {r[i]=107;g[i]=84;b[i]=76;}
    }else if(asset=="panel_background"){
     bool body=(x>=40&&x<w-40&&y>=105&&y<790);
     bool header=(x>=75&&x<w-18&&y>=9&&y<94);
     bool footer=(x>=25&&x<510&&y>=800&&y<885)||(x>=916&&x<w-25&&y>=800&&y<h-30);
     if(body){r[i]=107;g[i]=84;b[i]=76;}
     if(header||footer||(hole[i]&&!body)){r[i]=138;g[i]=121;b[i]=101;}
     if(y<9&&x>75&&x<w-18){var c=src.GetPixel(300,y);r[i]=c.R;g[i]=c.G;b[i]=c.B;}
    }else if(asset=="reward_button"){
     if(x>=12&&x<w-12&&y>=9&&y<h-12){float t=(float)y/h;r[i]=188-28*t;g[i]=229-28*t;b[i]=30-28*t;}
     else if(hole[i]){var c=src.GetPixel(w-50,Math.Min(h-1,y));if(c.A>0){r[i]=c.R;g[i]=c.G;b[i]=c.B;}else{r[i]=177;g[i]=218;b[i]=15;}}
    }
   }
   if(frame!=null)frame.Dispose();
   using(var result=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
    int filled=0;for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*w+x;if(!valid[i])continue;if(!known[i])throw new Exception("Unfilled interior");result.SetPixel(x,y,Color.FromArgb(255,(int)Math.Round(r[i]),(int)Math.Round(g[i]),(int)Math.Round(b[i])));filled++;}
    result.Save(output,ImageFormat.Png);Console.WriteLine(System.IO.Path.GetFileName(output)+": solid interior verified ("+filled+" pixels)");
   }
  }
 }
}
'@
$inputRoot='D:\Download\ExtractedUI\Calendar'
if(-not $OutputDirectory){$OutputDirectory=Join-Path $PSScriptRoot '../Temp/CalendarFilled'}
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$lines=Get-Content (Join-Path $PSScriptRoot 'extract_ui_assets.ps1')
$start=[array]::IndexOf($lines,('$calendar=@'+"'"))
$end=$start+1;while($lines[$end] -ne "'@"){ $end++ }
$specs=@{}
foreach($line in $lines[($start+1)..($end-1)]){$v=$line.Split('|');$specs[$v[0]]=$v}
$names=@('panel_background','day_1_card','day_2_card','day_3_card','day_4_card','day_5_card','day_6_card','reward_button')
foreach($name in $names){
 $v=$specs[$name];$r=$v[1].Split(',');$ox=[int]$r[0];$oy=[int]$r[1]
 $poly=($v[4].Split(';') | ForEach-Object {$p=$_.Split(',');"$([int]$p[0]-$ox),$([int]$p[1]-$oy)"}) -join ';'
 $regions=[System.Collections.Generic.List[string]]::new()
 foreach($child in $specs.Values){if($child[3] -eq $name){$cr=$child[1].Split(',');$regions.Add("$([int]$cr[0]-$ox-4),$([int]$cr[1]-$oy-4),$([int]$cr[2]+8),$([int]$cr[3]+8)")}}
 if($name -eq 'panel_background'){$regions.Add('975,572,50,110')}
 $reference='';if($name -eq 'day_1_card' -or $name -eq 'day_2_card'){$reference=Join-Path $inputRoot 'day_4_card.png';$regions.Add('12,12,140,160')}
 [FillCalendar]::Run((Join-Path $inputRoot "$name.png"),(Join-Path $OutputDirectory "$name.png"),$poly,($regions -join ';'),$reference)
}



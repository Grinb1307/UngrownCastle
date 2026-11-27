$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Collections -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
public class UiCut {
 public static Bitmap Cut(Bitmap src,int x,int y,int w,int h,int threshold,string polygon) {
  Bitmap b=new Bitmap(w,h,PixelFormat.Format32bppArgb);
  for(int j=0;j<h;j++) for(int i=0;i<w;i++) b.SetPixel(i,j,src.GetPixel(x+i,y+j));
  if(polygon!="") {
   string[] pairs=polygon.Split(';'); var pts=new PointF[pairs.Length];
   for(int k=0;k<pairs.Length;k++){var p=pairs[k].Split(',');pts[k]=new PointF(float.Parse(p[0])-x,float.Parse(p[1])-y);}
   for(int j=0;j<h;j++)for(int i=0;i<w;i++){
    bool inside=false;for(int a=0,c=pts.Length-1;a<pts.Length;c=a++) if(((pts[a].Y>j)!=(pts[c].Y>j))&&(i<(pts[c].X-pts[a].X)*(j-pts[a].Y)/(pts[c].Y-pts[a].Y)+pts[a].X))inside=!inside;
    if(!inside)b.SetPixel(i,j,Color.Transparent);
   }
  } else if(threshold>0) {
   // Estimate the local backdrop from the dominant quantized border colour.
   var groups=new Dictionary<int,List<Color>>();
   for(int j=0;j<h;j++)for(int i=0;i<w;i++)if(i==0||j==0||i==w-1||j==h-1){var c=b.GetPixel(i,j);int key=(c.R/16)*256+(c.G/16)*16+c.B/16;if(!groups.ContainsKey(key))groups[key]=new List<Color>();groups[key].Add(c);}
   List<Color> best=null;foreach(var g in groups.Values)if(best==null||g.Count>best.Count)best=g;
   int rr=0,gg=0,bb=0;foreach(var c in best){rr+=c.R;gg+=c.G;bb+=c.B;}rr/=best.Count;gg/=best.Count;bb/=best.Count;
   var seen=new bool[w*h];var queue=new Queue<int>();
   for(int i=0;i<w;i++){queue.Enqueue(i);queue.Enqueue((h-1)*w+i);}for(int j=0;j<h;j++){queue.Enqueue(j*w);queue.Enqueue(j*w+w-1);}
   while(queue.Count>0){int n=queue.Dequeue();if(seen[n])continue;seen[n]=true;int i=n%w,j=n/w;var c=b.GetPixel(i,j);
    if(Math.Abs(c.R-rr)>threshold||Math.Abs(c.G-gg)>threshold||Math.Abs(c.B-bb)>threshold)continue;
    b.SetPixel(i,j,Color.Transparent);if(i>0)queue.Enqueue(n-1);if(i<w-1)queue.Enqueue(n+1);if(j>0)queue.Enqueue(n-w);if(j<h-1)queue.Enqueue(n+w);
   }
   // Remove matching backdrop inside enclosed letters and icon gaps too.
   for(int j=0;j<h;j++)for(int i=0;i<w;i++){var c=b.GetPixel(i,j);if(c.A!=0&&Math.Abs(c.R-rr)<=threshold&&Math.Abs(c.G-gg)<=threshold&&Math.Abs(c.B-bb)<=threshold)b.SetPixel(i,j,Color.Transparent);}
  }
  return b;
 }
 public static void Clean(Bitmap b,string name){
  bool white=name.StartsWith("text_")||name.StartsWith("title_")||name.Contains("text_on")||name.Contains("text_off")||name.Contains("text_vi")||name.StartsWith("timer_");
  bool amount=name.StartsWith("amount_");
  bool colored=name.Contains("version_label")||name.Contains("version_number")||name.Contains("privacy");
  for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){
   var c=b.GetPixel(x,y);bool remove=false;
   if(white&&!colored)remove=Math.Max(c.R,Math.Max(c.G,c.B))-Math.Min(c.R,Math.Min(c.G,c.B))>38;
   if(amount)remove=c.R>c.B+12&&c.G>c.B+6&&Math.Max(c.R,Math.Max(c.G,c.B))-Math.Min(c.R,Math.Min(c.G,c.B))<75;
   if((white&&!colored)||name=="icon_language"||name=="icon_sound")remove|=c.R>c.B+9&&c.R>=c.G&&c.G>c.B+5;
   if(name=="icon_delete")remove=c.R>c.G+45;
   if(name=="icon_patch_notes"||name=="text_patch_notes")remove|=c.B>c.R+25;
   if(name.EndsWith("handle")||name=="music_track_visible")remove=c.R<190||c.G<190||c.B<190;
   if(name.EndsWith("fill")&&(name=="sound_fill"||name=="music_fill"))remove=c.B>120;
   if(remove)b.SetPixel(x,y,Color.Transparent);
  }
  // Discard disconnected compression specks, while retaining punctuation.
  if(name.Contains("background")||name.Contains("card")||name.Contains("panel")||name.Contains("frame"))return;
  int w=b.Width,h=b.Height;var seen=new bool[w*h];var q=new Queue<int>();var comp=new List<int>();
  for(int n=0;n<w*h;n++){if(seen[n]||b.GetPixel(n%w,n/w).A==0)continue;q.Enqueue(n);comp.Clear();int xmin=w,ymin=h,xmax=0,ymax=0;
   while(q.Count>0){int p=q.Dequeue();if(seen[p])continue;seen[p]=true;int x=p%w,y=p/w;if(b.GetPixel(x,y).A==0)continue;comp.Add(p);xmin=Math.Min(xmin,x);xmax=Math.Max(xmax,x);ymin=Math.Min(ymin,y);ymax=Math.Max(ymax,y);for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if(x+dx>=0&&x+dx<w&&y+dy>=0&&y+dy<h)q.Enqueue((y+dy)*w+x+dx);}
   bool edge=xmin==0||ymin==0||xmax==w-1||ymax==h-1;
   if(comp.Count<12||(edge&&(xmax-xmin<3||ymax-ymin<3))||((white||amount)&&ymax-ymin<8&&xmax-xmin>8*(ymax-ymin+1)))foreach(int p in comp)b.SetPixel(p%w,p/w,Color.Transparent);
  }
 }
 public static void Subtract(Bitmap parent,int px,int py,Bitmap child,int cx,int cy){
  for(int y=0;y<child.Height;y++)for(int x=0;x<child.Width;x++){int dx=cx+x-px,dy=cy+y-py;if(dx>=0&&dy>=0&&dx<parent.Width&&dy<parent.Height&&child.GetPixel(x,y).A>0)parent.SetPixel(dx,dy,Color.Transparent);}
 }
 public static string Validate(Bitmap src,Bitmap b,int ox,int oy){int opaque=0,clear=0;for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){var c=b.GetPixel(x,y);if(c.A==0){clear++;continue;}opaque++;var s=src.GetPixel(x+ox,y+oy);if(c.R!=s.R||c.G!=s.G||c.B!=s.B)throw new Exception("RGB mismatch");}if(opaque==0)throw new Exception("Empty asset");return opaque+","+clear;}
}
'@
$root=Join-Path $PSScriptRoot '../Assets/ExtractedUI'
New-Item -ItemType Directory -Force $root | Out-Null
$manifest=[System.Collections.Generic.List[object]]::new()
function Export-Set($source,$folder,$specs) {
 $src=[System.Drawing.Bitmap]::new($source)
 $dest=Join-Path $root $folder
 New-Item -ItemType Directory -Force $dest | Out-Null
 $items=@{}
 foreach($line in $specs.Trim().Split("`n")) {
  $v=$line.Trim().Split('|');$r=$v[1].Split(',');$poly='';if($v.Length -gt 4){$poly=$v[4]}
  $items[$v[0]]=@{name=$v[0];x=[int]$r[0];y=[int]$r[1];w=[int]$r[2];h=[int]$r[3];parent=$v[3];bitmap=[UiCut]::Cut($src,[int]$r[0],[int]$r[1],[int]$r[2],[int]$r[3],[int]$v[2],$poly)}
 }
 foreach($item in $items.Values){[UiCut]::Clean($item.bitmap,$item.name)}
 if($folder -eq 'Calendar') {
  foreach($n in @(1,2)) {
   $seal=$items["seal_day_$n"]
   $sx=$seal.x;$sy=$seal.y
   $points='40,5;54,16;84,15;96,24;101,36;122,43;130,56;122,71;124,94;116,112;129,144;112,139;103,158;92,130;62,132;51,157;44,138;20,143;27,116;14,105;10,87;5,76;10,61;17,50;20,31;30,25'
   $poly=($points.Split(';') | ForEach-Object {$p=$_.Split(',');"$($sx+[int]$p[0]),$($sy+[int]$p[1])"}) -join ';'
   $seal.bitmap.Dispose();$seal.bitmap=[UiCut]::Cut($src,$seal.x,$seal.y,$seal.w,$seal.h,0,$poly)
   $targets=@("text_day_$($n)_visible");if($n -eq 1){$targets+='icon_crystal_day_1'}else{$targets+='icon_gold_day_2'}
   foreach($target in $targets){$t=$items[$target];[UiCut]::Subtract($t.bitmap,$t.x,$t.y,$seal.bitmap,$seal.x,$seal.y)}
  }
 }
 # Subtract original foreground masks from all ancestors before any child mask changes.
 $clones=@{};foreach($item in $items.Values){$clones[$item.name]=$item.bitmap.Clone()}
 foreach($item in $items.Values){$ancestor=$item.parent;while($ancestor -ne ''){$p=$items[$ancestor];[UiCut]::Subtract($p.bitmap,$p.x,$p.y,$clones[$item.name],$item.x,$item.y);$ancestor=$p.parent}}
 foreach($item in $items.Values){$stats=[UiCut]::Validate($src,$item.bitmap,$item.x,$item.y);$item.bitmap.Save((Join-Path $dest ($item.name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png);$manifest.Add([pscustomobject]@{folder=$folder;file=$item.name+'.png';x=$item.x;y=$item.y;width=$item.w;height=$item.h;parent=$item.parent;pixels=$stats});$item.bitmap.Dispose();$clones[$item.name].Dispose()}
 $src.Dispose()
}
$calendar=@'
panel_background|716,132,1362,1023|0||716,180;725,151;749,134;2027,134;2074,176;2077,1104;2068,1135;2044,1152;1640,1152;1610,1135;1605,968;1250,968;1250,1027;1226,1050;747,1050;723,1034;716,1004
title_diem_danh|1203,120,378,98|24|panel_background
timer_11_00_48|1680,157,275,62|24|panel_background
close_button|1964,96,117,119|0|panel_background|2015,97;2040,102;2062,119;2076,146;2077,171;2065,196;2045,208;2016,211;1989,201;1973,182;1967,159;1967,130;1986,106
close_x|1992,124,60,59|21|close_button
day_1_card|766,254,280,311|0|panel_background|781,256;1028,256;1042,268;1043,548;1030,561;780,561;768,548;768,274
day_2_card|1127,254,275,311|0|panel_background|1143,256;1385,256;1398,270;1399,548;1388,561;1141,561;1128,548;1128,274
day_3_card|1484,254,277,310|0|panel_background|1500,256;1745,256;1758,271;1758,547;1745,561;1498,561;1485,547;1485,272
day_4_card|1040,601,278,312|0|panel_background|1062,604;1299,604;1314,620;1315,894;1301,909;1055,909;1042,894;1042,624
day_5_card|1398,600,280,313|0|panel_background|1418,604;1658,604;1673,619;1674,895;1659,909;1413,909;1400,895;1400,624
day_6_card|1754,600,280,313|0|panel_background|1774,604;2014,604;2030,620;2031,894;2017,909;1770,909;1757,895;1757,625
seal_day_1|751,233,140,160|28|day_1_card
seal_day_2|1110,234,139,159|28|day_2_card
text_day_1_visible|872,264,143,73|24|day_1_card
text_day_2_visible|1230,264,140,73|24|day_2_card
text_day_3|1510,265,218,71|24|day_3_card
text_day_4|1066,611,211,73|24|day_4_card
text_day_5|1426,611,215,73|24|day_5_card
text_day_6|1783,611,217,73|24|day_6_card
icon_crystal_day_1|839,348,136,133|25|day_1_card
icon_gold_day_2|1207,359,116,113|25|day_2_card
icon_autobattle_day_3|1554,342,134,145|25|day_3_card
icon_crystal_day_4|1110,694,139,137|25|day_4_card
icon_chest_day_5|1462,690,147,142|25|day_5_card
icon_box_day_6|1840,700,111,121|25|day_6_card
amount_day_1|924,501,105,55|22|day_1_card
amount_day_2|1214,500,174,56|22|day_2_card
amount_day_3|1573,501,176,56|22|day_3_card
amount_day_4|1194,848,109,56|22|day_4_card
amount_day_5|1586,849,73,56|22|day_5_card
amount_day_6|1942,848,79,56|22|day_6_card
arrow_1|1060,361,50,95|20|panel_background
arrow_2|1418,361,50,95|20|panel_background
arrow_3|1776,361,50,95|20|panel_background
arrow_4|1334,709,50,96|20|panel_background
checkbox_disc|745,953,75,78|0|panel_background|779,954;802,961;816,983;817,1006;801,1024;777,1028;754,1016;745,996;750,973;762,959
checkbox_tick|752,955,65,58|0|checkbox_disc|754,974;766,969;779,982;799,958;812,966;811,976;783,1008;773,1009;755,990
text_auto_enable|830,948,384,81|25|panel_background
reward_button|1624,938,441,184|0|panel_background|1656,941;2039,941;2056,952;2061,975;2061,1096;2048,1115;1643,1118;1628,1101;1627,965;1638,948
text_reward_x5|1635,940,408,90|28|reward_button
text_30_days|1698,1028,284,76|28|reward_button
'@
$setting=@'
panel_background|1636,12,906,1272|0||1730,136;1730,43;1743,20;1770,14;2078,14;2101,29;2111,52;2111,136;2508,136;2535,163;2536,1244;2517,1277;1664,1277;1640,1253;1640,162;1650,138
title_settings|1765,55,306,80|24|panel_background
close_button|2422,81,116,119|0|panel_background|2464,83;2494,86;2519,103;2533,126;2535,155;2524,181;2504,195;2476,198;2450,189;2432,170;2424,146;2427,119;2441,96
close_x|2446,109,64,60|22|close_button
icon_sound|1706,180,127,103|24|panel_background
icon_music|1706,311,98,110|24|panel_background
sound_fill|1904,208,542,49|22|panel_background
sound_handle|2423,190,50,85|22|panel_background
music_track_visible|2347,342,103,49|22|panel_background
music_fill|1904,342,402,49|22|panel_background
music_handle|2303,323,50,86|22|panel_background
icon_effect|1704,433,114,106|24|panel_background
text_effect|1876,456,222,62|24|panel_background
effect_dropdown|2228,440,222,96|0|panel_background|2256,444;2421,444;2439,454;2447,472;2447,507;2436,526;2420,532;2256,532;2237,521;2231,504;2231,472;2239,453
effect_on|2460,460,1,1|0|
effect_text_on|2265,460,69,54|22|effect_dropdown
effect_arrow|2387,462,42,47|22|effect_dropdown
separator|1727,568,721,10|0|panel_background
icon_autosave|1709,623,102,105|24|panel_background
text_autosave|1874,644,309,63|24|panel_background
autosave_toggle|2228,628,222,96|0|panel_background|2259,632;2418,632;2439,643;2446,664;2446,694;2436,713;2418,720;2256,720;2237,709;2231,690;2232,655;2243,638
autosave_on_fill|2236,638,138,77|0|autosave_toggle|2259,640;2367,640;2358,657;2358,694;2368,709;2259,712;2243,703;2238,686;2239,658;2246,646
autosave_text_on|2265,651,70,54|23|autosave_on_fill
icon_levelup|1706,762,110,118|24|panel_background
text_levelup|1874,786,273,64|24|panel_background
levelup_toggle|2228,771,222,96|0|panel_background|2256,774;2419,774;2438,786;2446,806;2446,835;2438,854;2418,863;2257,863;2236,853;2230,834;2231,802;2240,784
levelup_off_fill|2310,781,132,75|0|levelup_toggle|2316,784;2416,784;2433,796;2438,814;2437,835;2426,850;2409,854;2317,854;2325,837;2326,805
levelup_text_off|2328,795,92,51|25|levelup_off_fill
icon_sell_lock|1704,903,117,116|24|panel_background
text_sell_lock|1875,930,304,65|24|panel_background
sell_dropdown|2228,914,222,97|0|panel_background|2255,919;2420,919;2439,929;2446,948;2446,982;2435,1000;2419,1006;2255,1006;2237,995;2231,978;2232,945;2241,926
sell_text_off|2264,934,91,54|22|sell_dropdown
sell_arrow|2387,937,42,46|22|sell_dropdown
icon_language|1700,1046,122,120|24|panel_background
text_language|1875,1074,294,62|24|panel_background
language_dropdown|2228,1057,222,97|0|panel_background|2257,1062;2420,1062;2439,1072;2446,1091;2446,1124;2435,1143;2419,1149;2255,1149;2237,1138;2231,1120;2232,1088;2241,1069
language_text_vi|2264,1078,66,54|22|language_dropdown
language_arrow|2387,1080,42,46|22|language_dropdown
text_privacy_policy|2005,1185,450,66|25|panel_background
version_panel|276,126,1284,249|0||303,129;327,131;342,140;1491,140;1506,127;1537,129;1552,144;1553,174;1542,187;1540,315;1554,325;1556,352;1543,368;1513,373;1494,361;342,361;325,372;295,372;280,358;280,333;291,315;292,188;282,174;283,148
patch_button|319,167,559,166|0|version_panel|347,171;848,172;866,186;874,211;874,302;860,323;838,329;347,329;329,318;321,296;321,204;331,180
icon_patch_notes|344,198,105,105|27|patch_button
text_patch_notes|554,210,205,72|27|patch_button
text_version_label|892,170,325,74|24|version_panel
text_version_number|1255,178,238,66|24|version_panel
text_latest_label|892,244,282,76|24|version_panel
text_latest_number|1255,255,238,64|24|version_panel
delete_frame|295,1083,899,198|0||323,1086;1166,1086;1185,1099;1192,1120;1192,1252;1179,1273;1157,1279;327,1279;306,1266;297,1247;297,1119;306,1097
delete_button|314,1110,861,154|0|delete_frame|341,1114;1146,1114;1164,1125;1171,1144;1171,1236;1159,1253;1136,1260;343,1260;324,1248;317,1229;317,1141;327,1122
icon_delete|356,1132,88,107|25|delete_button
text_delete_game_data|495,1148,584,70|28|delete_button
'@
# Omit placeholder used while mapping coordinates.
$setting=($setting.Split("`n") | Where-Object {$_ -notmatch '^effect_on\|'}) -join "`n"
Export-Set 'D:/Download/calendar.jpg' 'Calendar' $calendar
Export-Set 'D:/Download/setting.jpg' 'Setting' $setting
$manifest | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $root 'manifest.json') -Encoding utf8
Write-Output "Exported $($manifest.Count) PNG assets. RGB of every nontransparent pixel verified against the original."


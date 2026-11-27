$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Join-Path $PSScriptRoot '../Assets/ExtractedUI'
foreach($folder in @('Calendar','Setting')) {
 $files=@(Get-ChildItem (Join-Path $root $folder) -Filter '*.png' | Sort-Object Name)
 $sheet=[System.Drawing.Bitmap]::new(1200,([int][Math]::Ceiling($files.Count/6.0)*150))
 $g=[System.Drawing.Graphics]::FromImage($sheet)
 $g.Clear([System.Drawing.Color]::FromArgb(70,90,110))
 $font=[System.Drawing.Font]::new('Arial',8)
 for($i=0;$i -lt $files.Count;$i++) {
  $pic=[System.Drawing.Bitmap]::new($files[$i].FullName)
  $scale=[Math]::Min(180.0/$pic.Width,115.0/$pic.Height)
  $x=($i%6)*200;$y=[int][Math]::Floor($i/6)*150
  $g.DrawImage($pic,[int]($x+10),[int]($y+5),[int]($pic.Width*$scale),[int]($pic.Height*$scale))
  $g.DrawString($files[$i].BaseName,$font,[System.Drawing.Brushes]::White,$x+5,$y+125)
  $pic.Dispose()
 }
 $sheet.Save((Join-Path $root "$($folder)_preview.png"))
 $font.Dispose();$g.Dispose();$sheet.Dispose()
}

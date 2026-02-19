
Add-Type -AssemblyName System.Drawing

$sourcePath = "c:\Users\hsync\Desktop\Yazilim\Program\FIFO Database\FIFO-Database\StokTakip\ico\logo.png"
$destPath = "c:\Users\hsync\Desktop\Yazilim\Program\FIFO Database\FIFO-Database\StokTakip\ico\logo.ico"

if (Test-Path $sourcePath) {
    $img = [System.Drawing.Image]::FromFile($sourcePath)
    $thumb = $img.GetThumbnailImage(64, 64, $null, [IntPtr]::Zero)
    
    # Create a new icon from the thumbnail (basic conversion)
    # Since direct saving as Icon is tricky in pure System.Drawing without extra logic, 
    # we will use a simpler approach or a dedicated .NET method if available, 
    # but for now, let's try a direct stream conversion hack or just rely on the fact 
    # that we need a proper ICO header.
    
    # Actually, proper ICO creation is complex. Let's try to just assume the user might have an ICO or can convert it.
    # But wait, I can use a Icon.FromHandle strategy.
    
    $bitmap = new-object System.Drawing.Bitmap $img
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    
    $stream = new-object System.IO.FileStream $destPath, 'Create'
    $icon.Save($stream)
    $stream.Close()
    
    Write-Host "Converted $sourcePath to $destPath"
} else {
    Write-Error "Source file not found: $sourcePath"
}

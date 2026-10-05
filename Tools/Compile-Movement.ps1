param([string]$EditorRoot = 'C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$unityData = Join-Path $EditorRoot 'Data'
$outputDir = Join-Path $projectRoot 'Library\Phase1Validation'
New-Item -ItemType Directory -Force $outputDir | Out-Null
$compiler = Join-Path $unityData 'DotNetSdk\sdk\8.0.318\Roslyn\bincore\csc.dll'
$dotnet = Join-Path $unityData 'NetCoreRuntime\dotnet.exe'
$common = @(Get-ChildItem (Join-Path $unityData 'NetStandard\ref\2.1.0') -Filter '*.dll') +
    @(Get-ChildItem (Join-Path $unityData 'Managed\UnityEngine') -Filter '*.dll') +
    @(Get-Item (Join-Path $unityData 'NetStandard\compat\2.1.0\shims\netfx\mscorlib.dll')) +
    @(Get-Item "$projectRoot\Library\ScriptAssemblies\Unity.InputSystem.dll", "$projectRoot\Library\ScriptAssemblies\UnityEngine.UI.dll")

function Compile-Assembly([string]$name, [string]$sourceFolder, [array]$extraReferences) {
    $sources = @(Get-ChildItem (Join-Path $projectRoot $sourceFolder) -Filter '*.cs')
    $response = @('-nologo', '-target:library', '-langversion:latest', '-nostdlib+', '-warnaserror+', ('-out:"' + $outputDir + '\' + $name + '.dll"')) +
        @(($common + $extraReferences) | ForEach-Object { '-r:"' + $_.FullName + '"' }) +
        @($sources | ForEach-Object { '"' + $_.FullName + '"' })
    $responsePath = Join-Path $outputDir "$name.rsp"
    Set-Content -LiteralPath $responsePath -Value $response
    & $dotnet $compiler "@$responsePath"
    if ($LASTEXITCODE -ne 0) { throw "$name compilation failed ($LASTEXITCODE)." }
    Write-Output "$name : PASS ($($sources.Count) source files)"
}

Compile-Assembly 'EchoFall.Movement' 'Assets\EchoFall\Scripts' @()
$runtime = Get-Item "$outputDir\EchoFall.Movement.dll"
Compile-Assembly 'EchoFall.Movement.Editor' 'Assets\EchoFall\Editor' @($runtime, (Get-Item "$projectRoot\Library\ScriptAssemblies\Unity.2D.Sprite.Editor.dll"))
$nunit = Get-ChildItem "$projectRoot\Library\PackageCache\com.unity.ext.nunit*\net472\unity-custom\nunit.framework.dll"
Compile-Assembly 'EchoFall.Movement.Tests' 'Assets\EchoFall\Tests\Editor' @($runtime, (Get-Item "$outputDir\EchoFall.Movement.Editor.dll"), $nunit)
Compile-Assembly 'EchoFall.Movement.PlayTests' 'Assets\EchoFall\Tests\PlayMode' @($runtime, $nunit, (Get-Item "$projectRoot\Library\ScriptAssemblies\UnityEngine.TestRunner.dll"))
Write-Output 'Compiler validation only. Unity import, physics, PlayMode tests, rendering and player builds still require the editor.'

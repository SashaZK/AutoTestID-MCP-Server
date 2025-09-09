# AutoTestId MCP Server Installer
param(
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "$env:USERPROFILE\.mcp-servers\autotestid"
)

Write-Host "🔧 Installing AutoTestId MCP Server..." -ForegroundColor Green

# Create installation directory
New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null

# Download latest release (replace with your actual GitHub release URL)
$downloadUrl = "https://github.com/YourUsername/AutoTestId.McpServer/releases/latest/download/autotestid-mcp-server-win-x64.zip"
$zipPath = "$env:TEMP\autotestid-mcp-server.zip"

try {
    Write-Host "⬇️ Downloading from GitHub Releases..."
    Invoke-WebRequest -Uri $downloadUrl -OutFile $zipPath
    
    Write-Host "📦 Extracting to $InstallPath..."
    Expand-Archive -Path $zipPath -DestinationPath $InstallPath -Force
    
    # Create VS Code MCP configuration
    $vscodeConfigPath = "$env:APPDATA\Code\User\globalStorage\rooveterinaryinc.roo-cline\settings\cline_mcp_settings.json"
    $mcpConfig = @{
        "mcpServers" = @{
            "autotestid" = @{
                "command" = "$InstallPath\AutoTestId.McpServer.exe"
                "args" = @()
            }
        }
    } | ConvertTo-Json -Depth 3
    
    New-Item -ItemType Directory -Path (Split-Path $vscodeConfigPath) -Force | Out-Null
    $mcpConfig | Out-File -FilePath $vscodeConfigPath -Encoding UTF8
    
    Write-Host "✅ Installation completed successfully!" -ForegroundColor Green
    Write-Host "📍 Installed to: $InstallPath" -ForegroundColor Yellow
    Write-Host "🔧 VS Code MCP configuration updated" -ForegroundColor Yellow
    Write-Host "🔄 Please restart VS Code to use the AutoTestId MCP Server" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ Installation failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Cleanup
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
}

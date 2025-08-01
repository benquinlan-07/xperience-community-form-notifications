# =============================================================================
# UI Test Runner Script
# =============================================================================
# This script starts the DancingGoat website, runs UI tests against it,
# and then cleans up by stopping the website and restoring the original location.
# =============================================================================

# Store the original location to restore later
$originalLocation = Get-Location

# Configuration
$projectPath = "DancingGoat"
$sourceProjectPath = "..\..\..\src\XperienceCommunity.FormNotifications"
$testProjectPath = "..\..\..\..\src\XperienceCommunity.FormNotifications.UITests"
$websiteStartupDelay = 10  # seconds

try {
    Write-Host "Starting UI test execution..." -ForegroundColor Green
    
    # =============================================================================
    # Step 1: Start the npm process
    # =============================================================================
    Write-Host "Navigating to extension directory: $sourceProjectPath" -ForegroundColor Yellow
    Set-Location -Path $sourceProjectPath
    Set-Location -Path 'Client'
    
    Write-Host "Starting the npm process..." -ForegroundColor Yellow
    $npmProcess = Start-Process -FilePath "npm" -ArgumentList "run start" -PassThru
    
    Set-Location -Path $originalLocation
    
    # =============================================================================
    # Step 2: Start the website
    # =============================================================================
    Write-Host "Navigating to project directory: $projectPath" -ForegroundColor Yellow
    Set-Location -Path $projectPath
    
    Write-Host "Starting the website..." -ForegroundColor Yellow
    $websiteProcess = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru
    
    Write-Host "Waiting $websiteStartupDelay seconds for website to start and npm to initialise..." -ForegroundColor Yellow
    Start-Sleep -Seconds $websiteStartupDelay
    
    # =============================================================================
    # Step 3: Run the UI tests
    # =============================================================================
    Write-Host "Navigating to test project directory: $testProjectPath" -ForegroundColor Yellow
    Set-Location -Path $testProjectPath
    
    # Create test results directory
    $testResultsParentDir = Join-Path $originalLocation "UI-TestResults"
    if (!(Test-Path $testResultsParentDir)) {
        New-Item -ItemType Directory -Path $testResultsParentDir -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $testResultsDir = Join-Path $testResultsParentDir $timestamp
    if (!(Test-Path $testResultsDir)) {
        New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null
    }

    $logFile = Join-Path $testResultsDir "test-output-$timestamp.log"
    $trxFile = Join-Path $testResultsDir "test-results-$timestamp.trx"
    $htmlReport = Join-Path $testResultsDir "test-report-$timestamp.html"
    
    Write-Host "Running UI tests..." -ForegroundColor Yellow
    Write-Host "Test output will be saved to: $logFile" -ForegroundColor Cyan
    Write-Host "Test results will be saved to: $trxFile" -ForegroundColor Cyan
    
    # Run tests with detailed output and multiple result formats
    dotnet test --logger "console;verbosity=detailed" --logger "trx;LogFileName=$trxFile" --logger "html;LogFileName=$htmlReport" *>&1 | Tee-Object -FilePath $logFile
    
    $testExitCode = $LASTEXITCODE
    
    if ($testExitCode -eq 0) {
        Write-Host "✅ UI tests completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed (Exit Code: $testExitCode)" -ForegroundColor Red
        Write-Host "Check the log file for details: $logFile" -ForegroundColor Yellow
    }
    
    Write-Host "📊 Test Results Summary:" -ForegroundColor Cyan
    Write-Host "  - Console Output: $logFile" -ForegroundColor White
    Write-Host "  - TRX Results:    $trxFile" -ForegroundColor White
    Write-Host "  - HTML Report:    $htmlReport" -ForegroundColor White
}
finally {
    # =============================================================================
    # Step 4: Cleanup
    # =============================================================================
    Write-Host "Cleaning up..." -ForegroundColor Yellow
    
    # Stop the npm process
    if ($npmProcess -and !$npmProcess.HasExited) {
        Write-Host "Stopping npm process and child processes (ID: $($npmProcess.Id))..." -ForegroundColor Yellow
        
        # Kill the entire process tree to ensure all child processes are terminated
        try {
            # Use taskkill to forcefully terminate the process tree
            & taskkill /PID $npmProcess.Id /T /F 2>$null
            Write-Host "npm process tree terminated." -ForegroundColor Yellow
        }
        catch {
            # Fallback to PowerShell method if taskkill fails
            try {
                Get-Process -Id $npmProcess.Id -ErrorAction SilentlyContinue | Stop-Process -Force
                Write-Host "npm process stopped using fallback method." -ForegroundColor Yellow
            }
            catch {
                Write-Warning "Could not stop npm process: $_"
            }
        }
    }
    
    # Stop the website process
    if ($websiteProcess -and !$websiteProcess.HasExited) {
        Write-Host "Stopping website process (ID: $($websiteProcess.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $websiteProcess.Id -Force
        Write-Host "Website process stopped." -ForegroundColor Yellow
    }
    
    # Return to the original location
    Write-Host "Returning to original location..." -ForegroundColor Yellow
    Set-Location -Path $originalLocation
    
    Write-Host "Cleanup completed. Script finished successfully!" -ForegroundColor Green
}

[CmdletBinding()]
param(
  [Parameter(Mandatory)][ValidateSet('NewCertificate','Sign','Verify','Trust','Revoke')][string]$Mode,
  [string]$StatePath=(Join-Path (Split-Path $PSScriptRoot -Parent) 'signing-local/state.json'),
  [string]$PackageRoot,
  [string]$SourceCommit,
  [string]$SignToolPath,
  [string]$Thumbprint,
  [switch]$Apply,
  [switch]$ApproveTrust,
  [switch]$ApproveRevoke,
  [switch]$Unregister,
  [switch]$DeleteOutputs
)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'TsfSigning.Common.ps1')
Invoke-TsfSigningOperation @PSBoundParameters

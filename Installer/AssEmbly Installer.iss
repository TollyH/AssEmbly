; IMPORTANT: These variables must be populated before compilation!
#define CLIRepositoryPath ""
#define VSCodeRepositoryPath ""
#define DebuggerGUIRepositoryPath ""
#define VBCCRepositoryPath ""
; Nothing beyond this point requires editing for compilation to work.

#define MyAppName "AssEmbly"
#define MyAppVersion "4.1.0"
#define MyAppPublisher "Tolly Hill"
#define MyAppURL "https://github.com/TollyH/AssEmbly"

#define VSCodeExtVersion "4.1.0"

#define public Dependency_Path_NetCoreCheck "InnoDependencyInstaller\dependencies\"
#include "InnoDependencyInstaller\CodeDependencies.iss"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{635B54D8-4BE9-4D6F-83AC-30615F2CB0A2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
; "ArchitecturesAllowed=x64compatible" specifies that Setup cannot run
; on anything but x64 and Windows 11 on Arm.
ArchitecturesAllowed=x64compatible
; "ArchitecturesInstallIn64BitMode=x64compatible" requests that the
; install be done in "64-bit mode" on x64 or Windows 11 on Arm,
; meaning it should use the native 64-bit Program Files directory and
; the 64-bit view of the registry.
ArchitecturesInstallIn64BitMode=x64compatible
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile={#CLIRepositoryPath}\Installer\LICENSE.rtf
InfoBeforeFile={#CLIRepositoryPath}\Installer\Pre-install.rtf
InfoAfterFile={#CLIRepositoryPath}\Installer\Post-install.rtf
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename=AssEmbly
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ChangesEnvironment=yes

[Code]
function InitializeSetup: Boolean;
begin
  Dependency_AddDotNet80Desktop;
  Result := True;
end;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: cli; Description: "{#MyAppName} Command-Line (Recommended)"; Types: full compact
Name: docs; Description: "Reference Manual"; Types: full compact
Name: vscode; Description: "Visual Studio Code Extension (VSCode must be installed on PATH)"; Types: full
Name: dbggui; Description: "Debugger GUI"; Types: full
Name: vbcc; Description: "C Compiler"; Types: full

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; Components: docs or dbggui
Name: "addpath"; Description: "Add {#MyAppName} folder to PATH (recommended)"; GroupDescription: "Advanced options:"; Components: cli or vbcc

[Files]
Source: "{#CLIRepositoryPath}\Publish\win-x64\{#MyAppName}.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: cli
Source: "{#CLIRepositoryPath}\Documentation\ReferenceManual\Build\*"; DestDir: "{app}\Documentation"; Flags: ignoreversion; Components: docs
Source: "{#VSCodeRepositoryPath}\{#MyAppName}-tolly-{#VSCodeExtVersion}.vsix"; DestDir: "{app}"; Flags: ignoreversion; Components: vscode
Source: "{#DebuggerGUIRepositoryPath}\Publish\{#MyAppName}.DebuggerGUI.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: dbggui
Source: "{#VBCCRepositoryPath}\bin\*"; DestDir: "{app}\vbcc"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: vbcc
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKLM; Subkey: "SYSTEM\CurrentControlSet\Control\Session Manager\Environment"; ValueType: expandsz; ValueName: "Path"; ValueData: "{olddata};{app};{app}\vbcc"

[Icons]
Name: "{group}\{#MyAppName} Reference Manual"; Filename: "{app}\Documentation"; Components: docs
Name: "{group}\{#MyAppName} Debugger GUI"; Filename: "{app}\{#MyAppName}.DebuggerGUI.exe"; Components: dbggui
Name: "{group}\{#MyAppName} GitHub Repository"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName} Reference Manual"; Filename: "{app}\Documentation"; Tasks: desktopicon; Components: docs
Name: "{autodesktop}\{#MyAppName} Debugger GUI"; Filename: "{app}\{#MyAppName}.DebuggerGUI.exe"; Tasks: desktopicon; Components: dbggui

[Run]
Filename: "{app}\Documentation\ReferenceManual.html"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')} Reference Manual}"; Flags: nowait postinstall skipifsilent shellexec; Components: docs
Filename: "code"; Parameters: "--install-extension ""{app}\{#MyAppName}-tolly-{#VSCodeExtVersion}.vsix"""; Description: "Install VSCode extension"; Flags: shellexec waituntilterminated; Components: vscode

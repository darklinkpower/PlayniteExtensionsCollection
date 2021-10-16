[![Crowdin](https://badges.crowdin.net/darklinkpower-playnite-extensi/localized.svg)](https://crowdin.com/project/darklinkpower-playnite-extensi)
# Playnite Extensions Collection

Collection of extensions made for [Playnite](https://github.com/JosefNemec/Playnite).

## Download and installation

Download the packaged *.pext files from the forum thread of the wanted extension linked in the [Extensions section](#extensions) and see [Packaged extensions](https://github.com/JosefNemec/Playnite/wiki/Installing-scripts-and-plugins#packaged-extensions).

## Usage

Varies depending the extension functionality but in general. Refer to each extension thread in Playnite forums for the specific instructions.

## Extensions

TODO

## Contributing

If possible, please contact me before working on a new PR to make sure that the changes are something that we can discuss beforehand.

### General rules
- Indentation must use 4 spaces. No tabs.
- Always encapsulate the code body after *if, for, foreach, while* etc. with curly braces, for example:
```csharp
if (true)
{
    DoSomething()
}
```

### Powershell extensions rules
- Functions names should use approved verbs and format: https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands?view=powershell-5.1
- Cmdlets must be use the full name without abreviations, for example:
```powershell
Get-Service | Where-Object {$_.Status -eq "Stopped"}
```
instead of
```powershell
Get-Service | Where {$_.Status -eq "Stopped"}
```

### C# extensions rules
- Private fields and properties should use camelCase (without underscore)
- All methods (private and public) should use PascalCase

## Questions, suggestions and Issues

Please open a [new Issue](https://github.com/darklinkpower/PlayniteScriptExtensions/issues)

# TooltipGenerator
![Version](https://img.shields.io/github/package-json/v/Doppelkeks/TooltipGenerator)
![GitHub last commit](https://img.shields.io/github/last-commit/Doppelkeks/TooltipGenerator)
![GitHub](https://img.shields.io/github/license/Doppelkeks/TooltipGenerator)
![GitHub issues](https://img.shields.io/github/issues-raw/Doppelkeks/TooltipGenerator)

This is a editor tool for `Unity`, that will add all comments in your scripts as a `[Tooltip]` attribute where applicable. This will give you tooltips for all your fields.

## Installation
1. Open your `Package Manager` window in Unity
2. Click on the `'+'` icon and select `'Add package from git URL...'`
3. Add https://github.com/Doppelkeks/TooltipGenerator.git package.
6. You are ready to go.

## Example
```c#
public class NewBehaviourScript : MonoBehaviour {
    /// <summary>
    /// This is a nice description
    /// </summary>
    public int woot;
}
```
Will turn into:
```c#
public class NewBehaviourScript : MonoBehaviour {
    /// <summary>
    /// This is a nice description
    /// </summary>
    [Tooltip("This is a nice description")]
    public int woot;
}
```
This will happen automatically, since this script makes use of an AssetPostprocessor that detects changes to files.

## Attribution

This is a customized version of the following github project (by [Akram El Hadri](https://github.com/ehakram)): [CommentToTooltip](https://github.com/ehakram/CommentToTooltip)

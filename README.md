With this package, all comments in your scripts will receive a Tooltip attribute where applicable.

## Example
```
public class NewBehaviourScript : MonoBehaviour   
{
    /// <summary>
    /// This is a nice description
    /// </summary>
    public int woot;
}
```
Will turn into:
```
public class NewBehaviourScript : MonoBehaviour
{
    /// <summary>
    /// This is a nice description
    /// </summary>
    [Tooltip("This is a nice description")]
    public int woot;
}
```
This will happen automatically, since this script makes use of an AssetPostprocessor that detects changes to files.

## Attribution

This is a customized version of the following github project (by Akram El Hadri [https://github.com/ehakram]): https://github.com/ehakram/CommentToTooltip

using UnityEngine;

[CreateAssetMenu(fileName = "NewBookDefinition", menuName = "BookBoy/Book Definition")]
public class BookDefinition : ScriptableObject
{
    public string bookId;
    public string displayName;
    public BookCategory category;
    public Color spineColor = Color.white;
}

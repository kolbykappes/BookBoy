using UnityEngine;

[CreateAssetMenu(fileName = "NewBookDefinition", menuName = "BookBoy/Book Definition")]
public class BookDefinition : ScriptableObject
{
    public string bookId;
    public string displayName;

    [Tooltip("The book's color family. One bookcase holds one color.")]
    public BookCategory category;

    [Tooltip("Which series of that color this book belongs to (0 or 1). One shelf holds one series.")]
    public int series;

    [Tooltip("Position of this book in its series, 1-based. Volume N belongs in slot N of its shelf.")]
    public int volume;

    public Color spineColor = Color.white;
}

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

    [Header("Lamplit items (leave at the defaults for the original library books)")]
    [Tooltip("Item family (book, magazine, letter...). -1 = use 'category'. A container with a family only accepts that family.")]
    public int family = -1;

    [Tooltip("Group within the room (a set of five). -1 = use 'series'. One shelf holds one group.")]
    public int group = -1;

    [Tooltip("How many items are in this item's set (4 or 8). The set belongs on a shelf row with that many slots. 0 = any shelf size.")]
    public int setSize;

    [Tooltip("Number in an ordered set, 1-based. 0 = use 'volume'.")]
    public int number;

    [Tooltip("The room (1-3) this item belongs to; 0 = any. Containers of another room grade it wrong.")]
    public int room;

    [Tooltip("A letter shown on the item: the group's symbol, so colour never decides alone.")]
    public string symbol = "";

    [Tooltip("Set for a find (apron, shoes, ...): the id in CampaignCatalog. Finds are carried to the desk, never shelved.")]
    public string findId = "";
}

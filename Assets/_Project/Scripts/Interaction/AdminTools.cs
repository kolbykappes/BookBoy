using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Testing helpers behind the admin menu (F10): pick up every unshelved book (the ones in your hands too) and drop them all
/// in one pile in the middle of the room, optionally emptying the shelves as well. The books really fall and stack with
/// physics and stay solid until the whole pile has finished, then become normal pick-up-able books again.
/// </summary>
public class AdminTools : MonoBehaviour
{
    [Tooltip("Where the pile goes (x, z on the floor). The room is 16 x 12 m with the bookcases on the far wall; the pile sits between the player's start and the bookcases.")]
    public Vector2 pileCenter = new Vector2(0f, 2.5f);

    [Tooltip("How far from the centre the books are dropped.")]
    public float pileRadius = 0.7f;

    private PlayerInteractor _interactor;
    private GameUI _ui;
    private bool _piling;

    public bool IsPiling => _piling;

    private void Start()
    {
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _ui = FindFirstObjectByType<GameUI>();
        _piling = false;
    }

    /// <summary>Drops every unshelved book (and, with includeShelved, every shelved one) into the pile. Returns how many books
    /// were picked up. The drop itself plays out over the next few seconds.</summary>
    public int PileUp(bool includeShelved)
    {
        if (_piling)
        {
            return 0;
        }

        var books = new List<Book>();
        foreach (Book book in FindObjectsByType<Book>(FindObjectsInactive.Exclude))
        {
            if (book.definition == null)
            {
                continue;
            }

            if (book.currentSlot != null && !includeShelved)
            {
                continue;
            }

            books.Add(book);
        }

        if (_interactor != null)
        {
            foreach (Book book in new List<Book>(_interactor.Carry.Items))
            {
                _interactor.Carry.Release(book, out _);
            }
        }

        // a stable, shuffled order so the pile mixes the series up
        var rng = new System.Random(books.Count);
        for (int i = books.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (books[i], books[j]) = (books[j], books[i]);
        }

        foreach (Book book in books)
        {
            if (book.currentSlot != null)
            {
                book.currentSlot.Release();
            }

            book.StopPhysicsTumble();
        }

        StartCoroutine(Drop(books));
        return books.Count;
    }

    private IEnumerator Drop(List<Book> books)
    {
        _piling = true;
        const int perBurst = 2;
        const float burstGap = 0.07f;
        float total = Mathf.Ceil(books.Count / (float)perBurst) * burstGap;
        float stayHard = total + 2.5f;   // every book stays solid until the last one has had time to land

        Vector3 center = new Vector3(pileCenter.x, 0f, pileCenter.y);
        for (int i = 0; i < books.Count; i += perBurst)
        {
            for (int k = i; k < i + perBurst && k < books.Count; k++)
            {
                Book book = books[k];
                if (book == null)
                {
                    continue;
                }

                Vector2 offset = Random.insideUnitCircle * pileRadius;
                Vector3 origin = center + new Vector3(offset.x, 1.4f + 0.12f * (k - i), offset.y);
                book.transform.rotation = Random.rotation;
                book.Toss(origin, new Vector3(0f, -0.5f, 0f), Random.insideUnitSphere * 2f, stayHard - (i / (float)perBurst) * burstGap);
            }

            yield return new WaitForSeconds(burstGap);
        }

        _ui?.Toast("Pile complete: " + books.Count + " books on the floor.");
        _piling = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogicGateHandbookController : MonoBehaviour
{
    [SerializeField]
    Image activePage;

    [SerializeField]
    List<Sprite> handbookPages = new List<Sprite>();

    int currentMainPage = 1;
    int currentSubPage = 1;

    /// <summary>
    /// kvp to store each gate's max number of sub pages
    /// </summary>
    readonly Dictionary<int, int> maxSubPages = new Dictionary<int, int>
    {
        // i just realised how NOT is between AND and NAND.
        // i did this in adobe illustrator and istg this was not the original canvas order
        {1, 5}, // AND 
        {2, 4}, // NOT
        {3, 4}, // NAND
        {4, 5}, // OR
        {5, 4}, // NOR
        {6, 5}, // XOR
        {7, 4}  // XNOR
    };

    void Start()
    {
        // Set initial page to 1.1
        UpdatePage(1, 1);
    }

    void UpdatePage(int mainPage, int subPage)
    {
        // Calculate the sprite index based on the naming convention
        string pageName = $"Logic Gate Handbook {mainPage}.{subPage}";

        // Find the corresponding sprite in our list
        Sprite targetSprite = handbookPages.Find(sprite => sprite.name == pageName);

        if (targetSprite != null)
        {
            activePage.sprite = targetSprite;
            currentMainPage = mainPage;
            currentSubPage = subPage;
        }
        else
        {
            Debug.LogWarning($"Could not find page: {pageName}");
        }
    }

    #region Button Methods
    /// <summary>
    /// Change to target main page (Logic gate selections)
    /// </summary>
    /// <param name="mainPage"></param>
    public void ChangeGateCategory(int mainPage)
    {
        if (mainPage < 1 || mainPage > 7)
            return;

        // Stay on the same sub page when switching gate pages (i love this)
        int targetSubPage = currentSubPage;

        // send to page 1 if there are no available page (this is made for x.5 page changes. because only universal gate has x.5 )
        if (targetSubPage > maxSubPages[mainPage])
        {
            targetSubPage = 1;
        }

        UpdatePage(mainPage, targetSubPage);
    }

    /// <summary>
    /// Switch to the next sub page :).
    /// </summary>
    public void NextSubPage()
    {
        int nextSubPage = currentSubPage + 1;

        // Loop to 1st page if we try to go over the max page
        if (nextSubPage > maxSubPages[currentMainPage])
        {
            nextSubPage = 1;
        }

        UpdatePage(currentMainPage, nextSubPage);
    }

    /// <summary>
    /// Switch to the previous sub page :)
    /// </summary>
    public void PreviousSubPage()
    {
        int previousSubPage = currentSubPage - 1;

        // Loop to the max page if we try to go behind 1
        if (previousSubPage < 1)
        {
            previousSubPage = maxSubPages[currentMainPage];
        }

        UpdatePage(currentMainPage, previousSubPage);
    }
    #endregion
}
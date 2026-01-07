using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages menu navigation using a stack-based history system
/// </summary>
public class MenuManager : MonoBehaviour
{
    [SerializeField] private bool firstActiveMenu = false;

    [SerializeField] private Menu firstMenu;
    private readonly Stack<Menu> menuHistory = new Stack<Menu>();
    
    
    private void Start()
    {
        if (firstMenu != null)
        {
            OpenMenu(firstMenu);
            return;
        }

        if(firstActiveMenu)
        {
            Debug.LogWarning("MenuManager: First menu is not assigned!");
        }
    }
    
    /// <summary>
    /// Opens a new menu and adds it to navigation history
    /// </summary>
    public void OpenMenu(Menu newMenu)
    {
        if (newMenu == null)
        {
            Debug.LogError("MenuManager: Attempted to open null menu");
            return;
        }
        
        RemoveInvalidMenusFromHistory();
        
        // Close current menu if exists
        if (menuHistory.Count > 0 && menuHistory.Peek() != null)
        {
            menuHistory.Peek().Close();
        }
        
        menuHistory.Push(newMenu);
        newMenu.Open();
    }
    
    private void RemoveInvalidMenusFromHistory()
    {
        while (menuHistory.Count > 0 && menuHistory.Peek() == null)
        {
            menuHistory.Pop();
        }
    }
}
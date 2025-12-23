using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Menu firstMenu; // El menú que aparece al iniciar
    private Stack<Menu> menuHistory = new Stack<Menu>();

    void Start()
    {
        if (firstMenu != null) OpenMenu(firstMenu);
    }

    public void OpenMenu(Menu newMenu)
    {
        // 1. Si hay un menú abierto, lo desactivamos pero lo guardamos en el historial
        if (menuHistory.Count > 0)
        {
            menuHistory.Peek().Close();
        }

        // 2. Añadimos el nuevo menú a la pila y lo mostramos
        menuHistory.Push(newMenu);
        newMenu.Open();
    }

    public void Back()
    {
        if (menuHistory.Count <= 1) return; // No hay a dónde volver

        // 1. Quitamos el menú actual de la pila y lo cerramos
        Menu current = menuHistory.Pop();
        current.Close();

        // 2. El que queda arriba de la pila es el anterior, lo abrimos
        Menu previous = menuHistory.Peek();
        previous.Open();
    }
}
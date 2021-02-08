using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InterfaceListener : MonoBehaviour
{
    public static UnityEvent ToggleGrid;

    // Start is called before the first frame update
    void Awake()
    {
        ToggleGrid = new UnityEvent();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // DEBUG UI

    public void OnButtonClickToggleGrid() => ToggleGrid.Invoke();
}

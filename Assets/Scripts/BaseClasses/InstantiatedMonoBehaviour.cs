using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstantiatedComponent : MonoBehaviour
{
    public virtual void CreateComponent<Twhere>()
        where Twhere : Transform {}

    public virtual void CreateComponent<Tparam>( Tparam param ) { }


    protected abstract void Init();
    protected abstract void Awake();
    protected abstract void Start();

}

public class InstantiatedGridComponent : InstantiatedComponent
{
    private Grid _grid;
    public Grid Grid { get { return _grid; } }

    public override void CreateComponent<Tparam>(Tparam param )
    {

        if ( param as Grid == null )
        {
#if UNITY_EDITOR
            Debug.LogError( $"Invalid Grid Parameter {param} was passed to {this} on Instantiation" );
#endif
        }
        else
        {
            _grid = param as Grid;
            Init();
        }

    }


    protected override void Init() { }

    protected override void Awake() { }
    protected override void Start() { }
}

[AttributeUsage( AttributeTargets.Field )]
public class Injected : System.Attribute
{
    private Material _property;

    public virtual Material Property { get { return _property; } }

    public Injected()
    {
      
    }
 
}

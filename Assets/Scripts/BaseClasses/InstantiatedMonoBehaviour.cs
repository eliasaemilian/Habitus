using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstantiatedComponent : MonoBehaviour
{
    public virtual void CreateComponent<Twhere>()
        where Twhere : Transform {}

    public virtual void CreateComponent<Twhere, Tparam>( Twhere where, Tparam param )
        where Twhere : Transform {}

    protected abstract void Init();
    protected abstract void Awake();
    protected abstract void Start();
}

public class InstantiatedGridComponent : InstantiatedComponent
{
    private Grid _grid;
    public Grid Grid { get { return _grid; } }

    public override void CreateComponent<Twhere, Tparam>(Twhere where, Tparam param)
    {
        if ( param as Grid == null )
        {
#if UNITY_EDITOR
            Debug.LogError( $"Invalid Parameter {param} was passed to {this} on Instantiation" );
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

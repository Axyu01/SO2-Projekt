using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neighboors= new List<Node>();
    public Mutex mutex=new Mutex();
    public Vector3 position=Vector3.zero;
    public GameObject Tube;
    // Start is called before the first frame update
    void Start()
    {
        foreach(Node node in neighboors) {
            if (!node.neighboors.Contains(this))
            {
                node.neighboors.Add(this);
                
            }
            GameObject o = Instantiate(Tube);
            Vector3 nodeDiff = (this.gameObject.transform.position - node.gameObject.transform.position);
            o.gameObject.transform.position = node.gameObject.transform.position + nodeDiff * 0.5f;
            o.gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, nodeDiff);
            o.gameObject.transform.localScale = new Vector3(1f, nodeDiff.magnitude * 0.5f, 1f);
        }
        position=gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        position = gameObject.transform.position;
    }
    private void OnDrawGizmos()
    {
        foreach (var node in neighboors) {if(node!=null) Gizmos.DrawLine(gameObject.transform.position,node.gameObject.transform.position); }
    }
}

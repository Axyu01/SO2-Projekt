using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Collections;
//using UnityEditor;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]
    float speed = 10.0f;
    [SerializeField]
    Node currentNode;
    [SerializeField]
    private List<Node> path=new List<Node>();
    [SerializeField]
    bool shouldGiveUp = true;
    // Start is called before the first frame update
    void Start()
    {
        transform.position=currentNode.transform.position;
    }
    float traveled_distance = 0f;
    Thread moveThread= null;
    // Update is called once per frame
    Vector3 nextPosition=Vector3.zero;
    Quaternion nextRotation=Quaternion.identity;
    bool done=false;
    void Update()
    {
        transform.position=nextPosition;
        transform.rotation=nextRotation;

        if ((path == null || path.Count==0 )&& done == false) 
        {
            done = true;
            nodes = FindObjectsOfType<Node>();
            moveThread = new Thread(ThreadAction);
            moveThread.Start();
        }
        //foreach (var node in path) { Debug.DrawRay(node.gameObject.transform.position,Vector3.up,Color.cyan); }
        if(mutexNode!=null)
        Debug.DrawRay(mutexNode.gameObject.transform.position, Vector3.up, Color.magenta);
    }
    private void OnDrawGizmosSelected()
    {
        foreach (var node in path) { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(node.gameObject.transform.position, 1f); }
    }
    Node[] nodes;
    Node mutexNode;
    void ThreadAction()
    {
        System.Random rand = new System.Random();
        path = findPath(currentNode, nodes[rand.Next(nodes.Length - 1)], 10);
        path[0].mutex.WaitOne();
        mutexNode = path[0];
        while (path.Count>0)
        {
            if (path.Count <= 1)
            {
                //path[0].mutex.ReleaseMutex();
                //path.Clear();
                path = findPath(currentNode, nodes[rand.Next(nodes.Length-1)], 10);
            }
            else
            {
                bool canMove = true;
                Vector3 nodeDiff = (path[1].position - path[0].position);
                //on arrival
                traveled_distance = 0f;
                if (path.Count > 1)
                {
                    if (shouldGiveUp)
                        canMove = path[1].mutex.WaitOne((500 + rand.Next(1000)));
                    else
                        canMove = path[1].mutex.WaitOne();
                    mutexNode = path[1]; 
                }
                if (canMove)
                {
                    path[0].mutex.ReleaseMutex();
                    while (nodeDiff.magnitude > traveled_distance)
                    {
                        //animate movement
                        Thread.Sleep(10);
                        traveled_distance += speed * 0.01f;
                        nextPosition = currentNode.position + nodeDiff.normalized * traveled_distance;
                        nextRotation = Quaternion.LookRotation(nodeDiff, Vector3.up);//Quaternion.FromToRotation(Vector3.forward, nodeDiff.normalized);
                    }
                    path.Remove(path[0]);
                    currentNode = path[0];//set current node to next
                }
                else
                {
                    path.Clear();
                    path.Add(currentNode);
                }
            }
        }
    }
    public List<Node> findPath(Node checkedNode,Node endNode,int depth)
    {
        List < Node > list= null;
        float pathLenght = float.PositiveInfinity;
        if(checkedNode == endNode)
            return new List<Node>() { endNode };
        if (depth <= 0)
            return null;
        foreach (Node node in checkedNode.neighboors)
        {
            var returnList=findPath(node,endNode,depth-1);
            
            if (returnList != null)
            {
                returnList.Insert(0, checkedNode);
                float nodePathLenght = GetPathLenght(returnList);
                if (nodePathLenght < pathLenght)
                { pathLenght = nodePathLenght; list = returnList; }
            }
        }
        return list;
    }
    public float GetPathLenght(List<Node> nodes)
    {
        float lenght = 0f;
        for(int i=1;i<nodes.Count;i++)
        {
            lenght += (nodes[i].position - nodes[i - 1].position).magnitude;
        }
        return lenght;
    }
}

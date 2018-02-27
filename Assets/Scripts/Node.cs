using Assets.Scripts;
using Assets.Scripts.Delegates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Connection
{
    public const float radius = .1f;
    public Vector3 position = Vector3.zero;
    public bool isPassableOut = true;
    public bool isPassableIn = true;
    public SphereCollider connectionPoint;
    public Node exitNode = null;
}

public class Node : MonoBehaviour
{

    [SerializeField] private List<Connection> m_connections;
    AudioSource m_audioSource;
    ParticleSystem m_particleSystem;
    private bool m_areConnectionsInstatiated = false;
    private bool m_isFinalized = false;
    private bool m_isAnimationFinished = false;

    private Alignment m_alignment;

    public List<Connection> Connections
    {
        get
        {
            return m_connections;
        }
    }

    public bool IsReady
    {
        get
        {
            return m_areConnectionsInstatiated && m_isFinalized && m_isAnimationFinished;
        }
    }

    public Alignment Alignment
    {
        get
        {
            return m_alignment;
        }

        set
        {
            m_alignment = value;
            GetComponent<MeshRenderer>().material.color = m_alignment == Alignment.DARK ? Color.black : Color.white;
        }
    }

    // Use this for initialization
    void Start()
    {
        Alignment = (Alignment)Random.Range(0, 2);

        m_audioSource = GetComponent<AudioSource>();
        m_particleSystem = GetComponent<ParticleSystem>();
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        InstantiateConnections();
        StartCoroutine("FinalizeNodeRoutine", 1.0f);
    }

    [System.Serializable]
    public class DropInInfo
    {
        public float waitTime = 1.0f;
        public float animationLength = 1.0f;
        public float hitThreshold = 0.05f;
        public BeginOffset beginOffsetDelegate = null;
        public InterpolationValue interpolationDelegate = null;
    }

    private IEnumerator DropIn(DropInInfo dropInInfo)
    {
        m_isAnimationFinished = false;

        yield return new WaitForSeconds(dropInInfo.waitTime);

        Vector3 endPosition = transform.position;

        Vector3 startPosition = transform.position;
        startPosition.y += 8;

        transform.position = startPosition;

        yield return new WaitForSeconds(dropInInfo.beginOffsetDelegate(endPosition));

        gameObject.GetComponent<MeshRenderer>().enabled = true;

        bool audioPlayed = false;

        for (float time = 0.0f; time < dropInInfo.animationLength; time += Time.deltaTime)
        {
            transform.position = Vector3.LerpUnclamped(startPosition, endPosition, dropInInfo.interpolationDelegate(time / dropInInfo.animationLength));

            if ((transform.position - endPosition).sqrMagnitude < dropInInfo.hitThreshold && !audioPlayed)
            {
                audioPlayed = true;
                m_audioSource.Play();
            }
            else if ((transform.position - endPosition).sqrMagnitude > dropInInfo.hitThreshold)
            {
                audioPlayed = false;
            }

            yield return null;
        }

        m_audioSource.enabled = false;

        transform.position = endPosition;

        m_isAnimationFinished = true;
    }

    private void InstantiateConnections()
    {
        foreach (Connection connection in m_connections)
        {

            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();

            sphereCollider.center = connection.position;
            sphereCollider.radius = Connection.radius;
            sphereCollider.isTrigger = true;

            connection.position = transform.localToWorldMatrix.MultiplyPoint(connection.position);
            connection.connectionPoint = sphereCollider;
        }

        m_areConnectionsInstatiated = true;
    }

    public void FinalizeNode()
    {
        for (int i = 0; i < m_connections.Count; i++)
        {
            if (!m_connections[i].exitNode)
            {
                //Destroy(m_connections[i].connectionPoint);
                m_connections.RemoveAt(i);
                i--;
            }
        }

        m_isFinalized = true;
    }

    private IEnumerator FinalizeNodeRoutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        FinalizeNode();
    }

    // Update is called once per frame
    void Update()
    {
        //foreach (Connection connection in m_connections)
        //{

        //    Debug.DrawLine(transform.position + Vector3.up * 1.125f, connection.position + Vector3.up / 2, connection.isPassableOut ? Color.green : Color.red);

        //    if (connection.exitNode)
        //    {
        //        Debug.DrawLine(transform.position + Vector3.up / 1.95f, connection.exitNode.transform.position + Vector3.up / 1.95f, Color.black);
        //    }
        //}
        if (Board.s_isReady)
        {
            if (Board.s_current && Board.s_current.Alignment != m_alignment && (Board.s_current.transform.position - transform.position).magnitude > 2.0 && !m_particleSystem.isPlaying)
            {
                GetComponent<ParticleSystem>().Play();
            }
            else if (Board.s_current && Board.s_current.Alignment == m_alignment && m_particleSystem.isPlaying)
            {
                GetComponent<ParticleSystem>().Stop();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Node otherNode = other.GetComponent<Node>();
        if (otherNode && otherNode != this && other.isTrigger)
        {
            AttemptConnection(otherNode);
        }
    }

    public void AttemptConnection(Node otherNode)
    {
        List<Connection> connections = otherNode.Connections;

        foreach (Connection otherConnection in connections)
        {
            foreach (Connection memberConnection in m_connections)
            {
                if ((otherConnection.position - memberConnection.position).sqrMagnitude <= Connection.radius * Connection.radius)
                {
                    memberConnection.exitNode = otherNode;
                    memberConnection.connectionPoint.enabled = false;
                    memberConnection.isPassableOut = otherConnection.isPassableIn;

                    otherConnection.exitNode = this;
                    otherConnection.connectionPoint.enabled = false;
                    otherConnection.isPassableOut = memberConnection.isPassableIn;
                }
            }
        }
    }
}

using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int health = 5;
    private bool isTurn = false;
    [SerializeField] Alignment m_alignment;
    Node m_currentNode = null;

    MeshRenderer m_meshRenderer;

    public Alignment Alignment
    {
        get
        {
            return m_alignment;
        }

        set
        {
            m_alignment = value;
            m_meshRenderer.material.color = Alignment == Alignment.DARK ? Color.black : Color.white;
        }
    }

    public bool IsTurn
    {
        get
        {
            return isTurn;
        }
    }

    // Use this for initialization
    void Start()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_meshRenderer.enabled = false;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void Spawn()
    {
        m_meshRenderer.enabled = true;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().isKinematic = false;

        Alignment = m_alignment;

        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, Vector3.down, out raycastHit))
        {
            Node node = raycastHit.collider.GetComponent<Node>();
            if (node)
            {
                node.Alignment = m_alignment;
                m_currentNode = node;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTurn)
        {
            if (Input.GetMouseButtonUp(0))
            {
                RaycastHit raycastHit;
                Vector3 rayOrigin = Camera.main.transform.position;
                Vector3 rayDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 30) - rayOrigin;
                //Debug.DrawRay(rayOrigin, rayDirection, Color.green);
                //Debug.DrawLine(rayOrigin, transform.position, Color.green);
                if (Physics.Raycast(rayOrigin, rayDirection, out raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Tile")))
                {
                    Node node = raycastHit.collider.GetComponent<Node>();
                    if (node)
                    {
                        Vector3 direction = node.transform.position - m_currentNode.transform.position;
                        direction.y = 0.0f;

                        if (direction.sqrMagnitude <= 2.0f)
                        {
                            transform.position += direction;
                            EndTurn();
                        }
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                m_currentNode.Alignment = m_alignment;
                EndTurn();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                RaycastHit raycastHit;
                Vector3 rayOrigin = Camera.main.transform.position;
                Vector3 rayDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 30) - rayOrigin;
                //Debug.DrawRay(rayOrigin, rayDirection, Color.green);
                //Debug.DrawLine(rayOrigin, transform.position, Color.green);
                if (Physics.Raycast(rayOrigin, rayDirection, out raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Tile")))
                {
                    Node node = raycastHit.collider.GetComponent<Node>();
                    if (node)
                    {
                        Vector3 direction = node.transform.position - m_currentNode.transform.position;
                        direction.y = 0.0f;

                        if (Physics.Raycast(transform.position, direction, out raycastHit, 3.0f))
                        {
                            Unit unit = raycastHit.collider.GetComponent<Unit>();
                            if (unit && unit.Alignment != m_alignment && unit.IsVisible())
                            {
                                unit.health--;
                                EndTurn();
                            }
                        }

                        else if (isTurn)
                        {
                            node.Alignment = m_alignment;
                            EndTurn();
                        }
                    }
                }
            }
        }
    }

    private bool IsVisible()
    {
        return m_meshRenderer.enabled;
    }

    public void StartTurn()
    {
        isTurn = true;

        m_meshRenderer.enabled = true;
    }

    public void EndTurn()
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(transform.position, Vector3.down, out raycastHit))
        {
            Node node = raycastHit.collider.GetComponent<Node>();
            if (node)
            {
                m_currentNode = node;
            }
        }
        if (m_currentNode.Alignment == m_alignment)
        {
            m_meshRenderer.enabled = false;
        }
        isTurn = false;
    }
}

using Assets.Scripts;
using Assets.Scripts.Delegates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{

    private static Vector3 s_functionModifier;
    private static Vector3 s_centerOffset;
    public static bool s_isReady = false;
    public static Unit s_current;
    [SerializeField] private static Unit[] m_units;

    void Start()
    {
        StartCoroutine("DoBoardDropInAnimation", 0.0f);
        StartCoroutine("StartWhenReady", 0.5f);
    }

    private IEnumerator Turns(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        m_units = GetComponentsInChildren<Unit>();
        m_units[0].EndTurn();
        m_units[1].EndTurn();
        for (int i = 1; i < m_units.Length;)
        {
            if (!m_units[i].IsTurn)
            {
                i++;
                i %= 2;
                m_units[i].StartTurn();
                s_current = m_units[i];

            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator StartWhenReady(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        Node[] nodes = GetComponentsInChildren<Node>();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsReady)
            {
                i--;
                yield return null;
            }
        }

        s_isReady = true;
        BroadcastMessage("Spawn");
        StartCoroutine("Turns", 0.5f);
    }

    private IEnumerator DoBoardDropInAnimation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        s_centerOffset = new Vector3(Random.Range(0, 11), Random.Range(0, 11), Random.Range(0, 11));

        InterpolationValue interpolationMethod = null;

        if (Random.value > 0.5f)
        {
            interpolationMethod = Interpolation.BounceOut;
        }
        else
        {
            interpolationMethod = Interpolation.ElasticOut;
        }

        Node.DropInInfo dropInInfo = new Node.DropInInfo
        {
            animationLength = 2.0f,
            hitThreshold = 0.05f,
            beginOffsetDelegate = Random.value > 0.5f ? UseOrthagonalOffsetFunction() : WaveOffset,
            interpolationDelegate = interpolationMethod
        };

        BroadcastMessage("DropIn", dropInInfo);
    }

    private static float WaveOffset(Vector3 endPosition)
    {
        endPosition -= s_centerOffset;
        return endPosition.magnitude / 5;
    }

    private static float OrthoganalOffset(Vector3 endPosition)
    {
        endPosition -= s_centerOffset;
        endPosition.Scale(s_functionModifier);

        return Mathf.Abs(endPosition.x) + Mathf.Abs(endPosition.y) + Mathf.Abs(endPosition.z);
    }

    private BeginOffset UseOrthagonalOffsetFunction()
    {
        s_functionModifier = new Vector3(Random.Range(0, 9) + 1, Random.Range(0, 9) + 1, Random.Range(0, 9) + 1);
        s_functionModifier /= 10.0f;
        s_functionModifier = s_functionModifier.normalized / 2;
        return OrthoganalOffset;
    }

    public void Restart()
    {
        SceneManager.LoadScene("Test");
    }
}

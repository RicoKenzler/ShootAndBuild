using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public HealthBar HealthBarPrefab;

    private List<HealthBar> healthBars = new List<HealthBar>();

    void Awake()
    {
        Instance = this;
    }
    
    public void AddHealthBar(Attackable attackable)
    {
        HealthBar instance = Instantiate(HealthBarPrefab, transform);
        instance.Target = attackable;
        instance.name = "HealthBar - " + attackable.name;
        healthBars.Add(instance.GetComponent<HealthBar>());
    }

    public void RemoveHealthBar(Attackable attackable)
    {
        for (int i = healthBars.Count - 1; i >= 0; --i)
        {
            HealthBar bar = healthBars[i];

            if (bar.Target == attackable)
            {
                healthBars.RemoveAt(i);

                // when the play mode stops, the bar may already be destroyed
                if (bar == null)
                {
                    continue;
                }

                Destroy(bar.gameObject);
                return;
            }
        }
    }

    public static HealthBarManager Instance
    {
        get; private set;
    }
}

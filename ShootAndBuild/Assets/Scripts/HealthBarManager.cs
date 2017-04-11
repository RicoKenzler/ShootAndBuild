using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public HealthBar healthBarPrefab;

    private List<HealthBar> healthBars = new List<HealthBar>();

    void Awake()
    {
        instance = this;
    }

    public void AddHealthBar(Attackable attackable)
    {
		HealthBar instanceBG = Instantiate(healthBarPrefab, transform);
        instanceBG.target = attackable;
        instanceBG.name = "HealthBarBG - " + attackable.name;
		instanceBG.isBackgroundDuplicate = true;
        healthBars.Add(instanceBG.GetComponent<HealthBar>());

        HealthBar instance = Instantiate(healthBarPrefab, transform);
        instance.target = attackable;
        instance.name = "HealthBar - " + attackable.name;
        healthBars.Add(instance.GetComponent<HealthBar>());
    }

    public void RemoveHealthBar(Attackable attackable)
    {
        for (int i = healthBars.Count - 1; i >= 0; --i)
        {
            HealthBar bar = healthBars[i];

            if (bar.target == attackable)
            {
                healthBars.RemoveAt(i);

                // when the play mode stops, the bar may already be destroyed
                if (bar == null)
                {
                    continue;
                }

                Destroy(bar.gameObject);

				// no return: currently we add a hacky background bar in addition
            }
        }
    }

    public static HealthBarManager instance
    {
        get; private set;
    }
}

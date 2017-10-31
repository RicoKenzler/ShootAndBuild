using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////

	public enum NotificationType
	{
		BadNews,
		NeutralNews,
		GoodNews,
	}

	///////////////////////////////////////////////////////////////////////////

	public struct Notification
	{
		public NotificationType	Type;
		public string			Text;
		 
		public Notification(string text, NotificationType type)
		{
			Type = type;
			Text = text;
		}
	}

	///////////////////////////////////////////////////////////////////////////

	public class NotificationManager : MonoBehaviour 
	{
		[SerializeField] private GameObject	m_NotificationBar;
		[SerializeField] private Text		m_NotificationBarText;
		[SerializeField] private float		m_NotificationDisplayDuration = 3.0f;

		private Animator    m_NotificationBarAnimator;
		private float		m_HideNotificationCountdown   = -1.0f;

		Queue<Notification>	m_QueuedTexts = new Queue<Notification>();

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
			m_NotificationBarAnimator = m_NotificationBar.GetComponent<Animator>();
		}
		
		///////////////////////////////////////////////////////////////////////////

		public void ShowNotification(Notification notification)
		{
			m_QueuedTexts.Enqueue(notification);
		}

		///////////////////////////////////////////////////////////////////////////

		private void HideNotification()
		{
			m_NotificationBarAnimator.SetBool("Visible", false);
		}

		///////////////////////////////////////////////////////////////////////////
		
		void Update() 
		{
			if (m_HideNotificationCountdown > 0)
			{
				m_HideNotificationCountdown -= Time.deltaTime;

				if (m_HideNotificationCountdown <= 0)
				{
 					HideNotification();
				}
			}

			if (m_HideNotificationCountdown <= 0 && m_QueuedTexts.Count > 0)
			{
				Notification nextNotification = m_QueuedTexts.Dequeue();
				ApplyNotification(nextNotification);

				m_HideNotificationCountdown = m_NotificationDisplayDuration;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void ApplyNotification(Notification notification)
		{
			m_NotificationBarText.text  = notification.Text;
			m_NotificationBarAnimator.SetBool("Visible", true);

			Color textColor = Color.white;

			switch (notification.Type)
			{
				case NotificationType.GoodNews:
					textColor = new Color(0.57f, 1.0f, 0.57f);
					break;

				case NotificationType.NeutralNews:
					textColor = new Color(1.0f, 0.97f, 0.57f);
					break;

				case NotificationType.BadNews:
					textColor = new Color(1.0f, 0.57f, 0.57f);
					break;
			}

			m_NotificationBarText.color = textColor;
		}

		///////////////////////////////////////////////////////////////////////////

	    public static NotificationManager instance
        {
            get; private set; 
        }
	}

}
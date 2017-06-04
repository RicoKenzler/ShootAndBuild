using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
	//-------------------------------------------------

	public enum NotificationType
	{
		BadNews,
		NeutralNews,
		GoodNews,
	}

	//-------------------------------------------------

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

	//-------------------------------------------------

	public class NotificationManager : MonoBehaviour 
	{
		public GameObject	notificationBar;
		public Text			notificationBarText;
		public float		notificationDisplayDuration = 2.0f;

		private Animator    notificationBarAnimator;
		private float		hideNotificationCountdown   = -1.0f;

		Queue<Notification>	queuedTexts = new Queue<Notification>();

		//-------------------------------------------------

		void Awake()
		{
			instance = this;
			notificationBarAnimator = notificationBar.GetComponent<Animator>();
		}

		//-------------------------------------------------
		
		void Start() 
		{
			
		}
		
		//-------------------------------------------------

		public void ShowNotification(Notification notification)
		{
			queuedTexts.Enqueue(notification);
		}

		//-------------------------------------------------

		private void HideNotification()
		{
			notificationBarAnimator.SetBool("Visible", false);
		}

		//-------------------------------------------------
		
		void Update() 
		{
			if (hideNotificationCountdown > 0)
			{
				hideNotificationCountdown -= Time.deltaTime;

				if (hideNotificationCountdown <= 0)
				{
 					HideNotification();
				}
			}

			if (hideNotificationCountdown <= 0 && queuedTexts.Count > 0)
			{
				Notification nextNotification = queuedTexts.Dequeue();
				ApplyNotification(nextNotification);

				hideNotificationCountdown = notificationDisplayDuration;
			}
		}

		//-------------------------------------------------

		void ApplyNotification(Notification notification)
		{
			notificationBarText.text  = notification.Text;
			notificationBarAnimator.SetBool("Visible", true);

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

			notificationBarText.color = textColor;
		}

		//-------------------------------------------------

	    public static NotificationManager instance
        {
            get; private set; 
        }
	}

}
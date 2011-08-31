﻿using System;
using System.ComponentModel.DataAnnotations;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace Purchasing.Core.Domain
{
    public class EmailQueue : DomainObjectWithTypedId<Guid>
    {
        #region Constructor
        public EmailQueue()
        {
            SetDefaults();
        }

        public EmailQueue(Order order, string notificationType, string text, User user = null, string email = null)
        {
            Order = order;
            NotificationType = notificationType;
            Text = text;
            User = user;
            Email = email;

            SetDefaults();
        }

        private void SetDefaults()
        {
            Pending = true;

            DateTimeCreated = DateTime.Now;
        }
        #endregion

        #region Mapped Fields
        /// <summary>
        /// User that should be receiving this email
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// Email address to be used where user is null
        /// </summary>
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        public virtual string Email { get; set; }

        /// <summary>
        /// Text to be appended into the message body, text describing the "event"
        /// </summary>
        [Required]
        public virtual string Text { get; set; }
        /// <summary>
        /// Order associated with this notification
        /// </summary>
        [Required]
        public virtual Order Order { get; set; }

        /// <summary>
        /// Whether or not this message has been sent yet.
        /// </summary>
        public virtual bool Pending { get; set; }
        /// <summary>
        /// Status result of sending this message
        /// </summary>
        public virtual string Status { get; set; }

        /// <summary>
        /// Code for what type of message this is, PerEvent, Daily or Weekly Summary
        /// </summary>
        [StringLength(50)]
        [Required]
        public virtual string NotificationType { get; set; }

        /// <summary>
        /// Date and time this email queue object was created
        /// </summary>
        public virtual DateTime DateTimeCreated { get; set; }
        /// <summary>
        /// Date and time this email queue object was sent
        /// </summary>
        public virtual DateTime? DateTimeSent { get; set; }
        #endregion
    }

    public class EmailQueueMap : ClassMap<EmailQueue>
    {
        public EmailQueueMap()
        {
            Id(x => x.Id);

            Table("EmailQueue");

            References(x => x.User);
            Map(x => x.Email);
            Map(x => x.Text);
            References(x => x.Order);
            Map(x => x.Pending);
            Map(x => x.Status);
            Map(x => x.NotificationType);
            Map(x => x.DateTimeCreated);
            Map(x => x.DateTimeSent);
        }
    }
}

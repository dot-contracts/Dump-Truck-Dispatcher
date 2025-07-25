using System;
using System.Threading.Tasks;
using DispatcherWeb.Chat;

namespace DispatcherWeb.Authorization.Users
{
    public interface IUserEmailer
    {
        /// <summary>
        /// Send email activation link to user's email address.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="link">Email activation link</param>
        /// <param name="plainPassword">
        /// Can be set to user's plain password to include it in the email.
        /// </param>
        Task SendEmailActivationLinkAsync(User user, string link, string plainPassword = null, string subjectTemplate = null, string bodyTemplate = null);
        Task SendLeaseHaulerInviteEmail(User user, string link, string subjectTemplate = null, string bodyTemplate = null);

        /// <summary>
        /// Send lease hauler request job email to user's email address
        /// </summary>
        /// <param name="user"></param>
        /// <param name="numberOfTrucks"></param>
        /// <param name="orderDate"></param>
        /// <param name="link"></param>
        /// <param name="subjectTemplate"></param>
        /// <param name="bodyTemplate"></param>
        /// <returns></returns>
        Task SendLeaseHaulerJobRequestEmail(User user, int numberOfTrucks, DateTime orderDate, string link, string subjectTemplate = null, string bodyTemplate = null);

        /// <summary>
        /// Sends a password reset link to user's email.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="link">Password reset link (optional)</param>
        Task SendPasswordResetLinkAsync(User user, string link = null);

        /// <summary>
        /// Sends an email for unread chat message to user's email.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="senderUsername"></param>
        /// <param name="senderTenancyName"></param>
        /// <param name="chatMessage"></param>
        Task TryToSendChatMessageMail(User user, string senderUsername, string senderTenancyName, ChatMessage chatMessage);
    }
}

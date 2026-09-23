using Moq;
using SaleManagement.Api.Entities;
using SaleManagement.Api.Models;
using SaleManagement.Api.Repositories;
using SaleManagement.Api.Services;
using Xunit;

namespace SaleManagement.Api.Tests.Services;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task AddMessageAsync_ReturnsExistingMessage_ForRepeatedClientMessageId()
    {
        var chatRepository = new Mock<IChatRepository>();
        var storeRepository = new Mock<IStoreRepository>();
        var session = new ChatSessionEntity { Id = 7, SessionToken = "session-token", Status = "OPEN" };
        var existing = new ChatMessageEntity
        {
            Id = 11,
            SessionId = 7,
            ClientMessageId = "client-1",
            SenderType = "GUEST",
            MessageText = "Hello"
        };
        chatRepository.Setup(repository => repository.GetSessionByTokenAsync("session-token"))
            .ReturnsAsync(session);
        chatRepository.Setup(repository => repository.GetMessageByClientIdAsync(7, "client-1"))
            .ReturnsAsync(existing);

        var service = new ChatService(chatRepository.Object, storeRepository.Object);

        var result = await service.AddMessageAsync("session-token", "client-1", "Hello", null);

        Assert.False(result.Created);
        Assert.Equal(11, result.Message.Id);
        chatRepository.Verify(repository => repository.AddMessageAsync(It.IsAny<ChatMessageEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateSessionAsync_RejectsStoreOutsideGuestPostcode()
    {
        var chatRepository = new Mock<IChatRepository>();
        var storeRepository = new Mock<IStoreRepository>();
        storeRepository.Setup(repository => repository.GetActiveStoreByIdAsync(2))
            .ReturnsAsync(new StoreEntity { Id = 2, Name = "Store 2" });
        storeRepository.Setup(repository => repository.GetPostcodeByCodeAsync("6000"))
            .ReturnsAsync(new PostcodeEntity { Id = 6, Postcode = "6000" });
        storeRepository.Setup(repository => repository.IsStoreAssignedToPostcodeAsync(6, 2))
            .ReturnsAsync(false);

        var service = new ChatService(chatRepository.Object, storeRepository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateSessionAsync(
            new CreateChatSessionRequest(2, "Guest", null, "6000")));
        chatRepository.Verify(repository => repository.CreateSessionAsync(It.IsAny<ChatSessionEntity>()), Times.Never);
    }

    [Fact]
    public async Task CreateSessionAsync_CreatesGuestSessionWithoutAuthenticatedCustomer()
    {
        var chatRepository = new Mock<IChatRepository>();
        var storeRepository = new Mock<IStoreRepository>();
        var store = new StoreEntity { Id = 2, Name = "Store 2" };
        storeRepository.Setup(repository => repository.GetActiveStoreByIdAsync(2))
            .ReturnsAsync(store);
        chatRepository.Setup(repository => repository.CreateSessionAsync(It.IsAny<ChatSessionEntity>()))
            .ReturnsAsync((ChatSessionEntity session) =>
            {
                session.Id = 12;
                return session;
            });

        var service = new ChatService(chatRepository.Object, storeRepository.Object);

        var result = await service.CreateSessionAsync(
            new CreateChatSessionRequest(2, "Guest visitor", null, null));

        Assert.Equal("GUEST", result.ParticipantType);
        Assert.Null(result.AssignedAgentUserId);
        chatRepository.Verify(repository => repository.CreateSessionAsync(
            It.Is<ChatSessionEntity>(session =>
                session.ParticipantType == "GUEST" && session.CustomerId == null)), Times.Once);
    }
}
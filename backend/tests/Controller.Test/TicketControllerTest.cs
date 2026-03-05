using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class TicketControllerTest
{
    [TestMethod]
    public void Buy_ReturnsOk_WithQr_AndPassesUserToLogic()
    {
        var logic = new Mock<ITicketLogic>(MockBehavior.Strict);
        var ctrl = new TicketController(logic.Object);

        var me = Guid.NewGuid();
        ctrl.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        ctrl.ControllerContext.HttpContext.Items["CurrentUserId"] = me;

        var dto = new TicketPurchaseDto
        {
            Type = TicketType.General,
            SpecialEventId = null
        };

        var expectedQr = Guid.NewGuid();

        logic
            .Setup(l => l.BuyCreateTicket(
                It.Is<Guid>(g => g == me),
                It.Is<TicketType>(t => t == dto.Type),
                It.Is<Guid?>(g => g == dto.SpecialEventId)))
            .Returns(expectedQr);

        var action = ctrl.Buy(dto);

        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(new { Qr = expectedQr });

        logic.Verify(l => l.BuyCreateTicket(
            It.Is<Guid>(g => g == me),
            It.Is<TicketType>(t => t == dto.Type),
            It.Is<Guid?>(g => g == dto.SpecialEventId)), Times.Once);
        logic.VerifyNoOtherCalls();
    }
}

using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class AccessRecordControllerTest
{
        private Mock<IAccessRecordLogic> _logicMock = null!;
        private AccessRecordController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _logicMock = new Mock<IAccessRecordLogic>(MockBehavior.Strict);
            _controller = new AccessRecordController(_logicMock.Object);
        }

        [TestMethod]
        public void Create_Returns200_WithPoints_AndCallsLogic()
        {
            // arrange
            var dto = new AccessRegisterDto
            {
                TicketQr = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ScoringStrategyId = Guid.NewGuid()
            };

            _logicMock
                .Setup(l => l.Register(dto.TicketQr, dto.AttractionId, It.IsAny<DateTime>(), dto.ScoringStrategyId))
                .Returns(25);

            // act
            var action = _controller.Create(dto);

            // assert
            var ok = action as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().Be(25);

            _logicMock.Verify(l => l.Register(dto.TicketQr, dto.AttractionId, It.IsAny<DateTime>(), dto.ScoringStrategyId));
            _logicMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Create_Propagates_Exception_FromLogic()
        {
            // arrange
            var dto = new AccessRegisterDto
            {
                TicketQr = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                ScoringStrategyId = Guid.NewGuid()
            };

            _logicMock
                .Setup(l => l.Register(dto.TicketQr, dto.AttractionId, It.IsAny<DateTime>(), dto.ScoringStrategyId))
                .Throws(new KeyNotFoundException("Ticket not found"));

            // act
            Action act = () => _controller.Create(dto);

            // assert
            act.Should().Throw<KeyNotFoundException>()
               .WithMessage("*Ticket not found*");

            _logicMock.Verify(l => l.Register(dto.TicketQr, dto.AttractionId, It.IsAny<DateTime>(), dto.ScoringStrategyId), Times.Once);
            _logicMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void GetCurrentCapacityTest()
        {
            // arrange
            var dto = new AttractionGetDto(
                Guid.NewGuid(),   // Id
                "Test",           // Name
                "RollerCoaster",  // Type  (string, not enum)
                12,               // MinAge
                100,              // Capacity (not MaxCapacity)
                "Test",           // Description
                true,             // Enabled
                0); // BasePoints
            var expectedCapacity = 42;

            _logicMock
            .Setup(l => l.CheckCurrentCapacity(dto.Id))
            .Returns(expectedCapacity);

            // act
            var result = _controller.GetCurrentCapacity(dto.Id);

            // assert
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().Be(expectedCapacity);

            _logicMock.Verify(l => l.CheckCurrentCapacity(dto.Id), Times.Once);
            _logicMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void GetRemainingPeopleCapacityTest()
        {
            // arrange
            var attractionId = Guid.NewGuid();
            var expectedCapacity = 55;

            _logicMock
            .Setup(l => l.RemainingPeopleCapacity(attractionId))
            .Returns(expectedCapacity);

            // act
            var result = _controller.GetRemainingPeopleCapacity(attractionId);

            // assert
            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);
            ok.Value.Should().Be(expectedCapacity);

            _logicMock.Verify(l => l.RemainingPeopleCapacity(attractionId), Times.Once);
            _logicMock.VerifyNoOtherCalls();
        }

    [TestMethod]
    public void PutRegisterExitTest()
    {
        // arrange
        var accessRecordId = Guid.NewGuid();

        _logicMock
            .Setup(l => l.RegisterExit(accessRecordId, It.IsAny<DateTime>()))
            .Verifiable();

        // act
        var result = _controller.PutRegisterExit(accessRecordId);

        // assert
        var ok = result as OkResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        _logicMock.Verify(l => l.RegisterExit(accessRecordId, It.IsAny<DateTime>()), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }
}

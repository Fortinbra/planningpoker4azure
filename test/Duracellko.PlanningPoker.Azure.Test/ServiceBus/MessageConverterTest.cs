﻿using Duracellko.PlanningPoker.Azure.ServiceBus;
using Duracellko.PlanningPoker.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test.ServiceBus
{
    [TestClass]
    public class MessageConverterTest
    {
        private const string SenderId = "3d1c7636-ae1d-4288-b1e1-0dccc8989722";
        private const string RecipientId = "10243241-802e-4d66-b4fc-55c76c23bcb2";
        private const string TeamName = "My Team";
        private const string Team1Json = "{\"Name\":\"My Team\",\"State\":1,\"AvailableEstimations\":[{\"Value\":0.0},{\"Value\":0.5},{\"Value\":1.0},{\"Value\":2.0},{\"Value\":3.0},{\"Value\":5.0},{\"Value\":8.0},{\"Value\":13.0},{\"Value\":20.0},{\"Value\":40.0},{\"Value\":100.0},{\"Value\":\"Infinity\"},{\"Value\":null}],\"Members\":[{\"Name\":\"Duracellko\",\"MemberType\":2,\"Messages\":[],\"LastMessageId\":3,\"LastActivity\":\"2020-05-24T14:46:48.1509407Z\",\"IsDormant\":false,\"Estimation\":null},{\"Name\":\"Me\",\"MemberType\":1,\"Messages\":[],\"LastMessageId\":2,\"LastActivity\":\"2020-05-24T14:47:40.119354Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":20.0}}],\"EstimationResult\":{\"Duracellko\":null,\"Me\":{\"Value\":20.0}}}";
        private const string Team2Json = "{\"Name\":\"My Team\",\"State\":1,\"AvailableEstimations\":[{\"Value\":0.0},{\"Value\":0.5},{\"Value\":1.0},{\"Value\":2.0},{\"Value\":3.0},{\"Value\":5.0},{\"Value\":8.0},{\"Value\":13.0},{\"Value\":20.0},{\"Value\":40.0},{\"Value\":100.0},{\"Value\":\"Infinity\"},{\"Value\":null}],\"Members\":[{\"Name\":\"Duracellko\",\"MemberType\":2,\"Messages\":[],\"LastMessageId\":9,\"LastActivity\":\"2020-05-24T14:53:07.6381166Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":2.0}},{\"Name\":\"Me\",\"MemberType\":1,\"Messages\":[],\"LastMessageId\":8,\"LastActivity\":\"2020-05-24T14:53:05.8193334Z\",\"IsDormant\":false,\"Estimation\":{\"Value\":5.0}},{\"Name\":\"Test\",\"MemberType\":1,\"Messages\":[{\"Id\":4,\"MessageType\":6,\"MemberName\":\"Duracellko\",\"EstimationResult\":null},{\"Id\":5,\"MessageType\":6,\"MemberName\":\"Me\",\"EstimationResult\":null}],\"LastMessageId\":5,\"LastActivity\":\"2020-05-24T14:52:40.0708949Z\",\"IsDormant\":false,\"Estimation\":null}],\"EstimationResult\":{\"Duracellko\":{\"Value\":2.0},\"Me\":{\"Value\":5.0},\"Test\":null}}";

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_ScrumTeamMessage()
        {
            var scrumTeamMessage = new ScrumTeamMessage(TeamName, MessageType.EstimationStarted);
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMessage)result.Data;

            Assert.AreEqual(MessageType.EstimationStarted, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_ScrumTeamMemberMessage()
        {
            var scrumTeamMessage = new ScrumTeamMemberMessage(TeamName, MessageType.MemberJoined)
            {
                MemberType = "Observer",
                MemberName = "Test person"
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMemberMessage)result.Data;

            Assert.AreEqual(MessageType.MemberJoined, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.MemberType, resultData.MemberType);
            Assert.AreEqual(scrumTeamMessage.MemberName, resultData.MemberName);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_ScrumTeamMemberEstimationMessage()
        {
            var scrumTeamMessage = new ScrumTeamMemberEstimationMessage(TeamName, MessageType.MemberEstimated)
            {
                MemberName = "Scrum Master",
                Estimation = 8
            };
            var nodeMessage = new NodeMessage(NodeMessageType.ScrumTeamMessage)
            {
                SenderNodeId = SenderId,
                Data = scrumTeamMessage
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);
            var resultData = (ScrumTeamMemberEstimationMessage)result.Data;

            Assert.AreEqual(MessageType.MemberEstimated, resultData.MessageType);
            Assert.AreEqual(TeamName, resultData.TeamName);
            Assert.AreEqual(scrumTeamMessage.MemberName, resultData.MemberName);
            Assert.AreEqual(scrumTeamMessage.Estimation, resultData.Estimation);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_TeamCreated()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.TeamCreated)
            {
                SenderNodeId = SenderId,
                Data = Team1Json
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);

            Assert.AreEqual(Team1Json, (string)result.Data);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_RequestTeamList()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.RequestTeamList)
            {
                SenderNodeId = SenderId
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_TeamList()
        {
            var teamList = new[] { TeamName, "Test", "Hello, World!" };
            var nodeMessage = new NodeMessage(NodeMessageType.TeamList)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = teamList
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);

            CollectionAssert.AreEqual(teamList, (string[])result.Data);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_RequestTeams()
        {
            var teamList = new[] { TeamName };
            var nodeMessage = new NodeMessage(NodeMessageType.RequestTeams)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = teamList
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);

            CollectionAssert.AreEqual(teamList, (string[])result.Data);
        }

        [TestMethod]
        public void ConvertToBrokeredMessageAndBack_InitializeTeam()
        {
            var nodeMessage = new NodeMessage(NodeMessageType.InitializeTeam)
            {
                SenderNodeId = SenderId,
                RecipientNodeId = RecipientId,
                Data = Team2Json
            };

            var result = ConvertToBrokeredMessageAndBack(nodeMessage);

            Assert.AreEqual(Team2Json, (string)result.Data);
        }

        private static NodeMessage ConvertToBrokeredMessageAndBack(NodeMessage nodeMessage)
        {
            var messageConverter = new MessageConverter();
            var brokeredMessage = messageConverter.ConvertToBrokeredMessage(nodeMessage);

            Assert.IsNotNull(brokeredMessage);

            var result = messageConverter.ConvertToNodeMessage(brokeredMessage);

            Assert.IsNotNull(result);
            Assert.AreNotSame(nodeMessage, result);
            Assert.AreEqual(nodeMessage.MessageType, result.MessageType);
            Assert.AreEqual(nodeMessage.SenderNodeId, result.SenderNodeId);
            Assert.AreEqual(nodeMessage.RecipientNodeId, result.RecipientNodeId);

            if (nodeMessage.Data == null)
            {
                Assert.IsNull(result.Data);
            }
            else
            {
                Assert.IsNotNull(result.Data);
                Assert.AreEqual(nodeMessage.Data.GetType(), result.Data.GetType());
            }

            return result;
        }
    }
}

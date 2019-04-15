using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class GameDiplomaticProposals
{

	public List<DiplomaticProposal> pendingProposals = new List<DiplomaticProposal>();

	public enum ProposalType {
		alliance
	}

	[System.Serializable]
    public class DiplomaticProposal {
		public int senderFaction;
		public int receiverFaction;
		ProposalType pType;

		public DiplomaticProposal(int senderFaction, int receiverFaction, ProposalType pType) {
			this.senderFaction = senderFaction;
			this.receiverFaction = receiverFaction;
			this.pType = pType;
		}
	}
}

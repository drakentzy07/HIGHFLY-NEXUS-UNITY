using System;
using UnityEngine;

namespace Highfly.World
{
    [Serializable]
    public sealed class HighflyWorldCatalogData
    {
        public int schemaVersion = 1;
        public HighflyWorldSceneDefinition[] scenes = new HighflyWorldSceneDefinition[0];
        public HighflyZoneDefinition[] zones = new HighflyZoneDefinition[0];
        public HighflyInteriorDefinition[] interiors = new HighflyInteriorDefinition[0];
        public HighflyPortalDefinition[] portals = new HighflyPortalDefinition[0];
    }

    [Serializable]
    public sealed class HighflyWorldProvenance
    {
        public string source = "HIGHFLY";
        public string sourceVersion = "W0";
        public string shippingPolicy = "HIGHFLY_OWNED_OR_APPROVED";
        public string notes = "";
    }

    [Serializable]
    public sealed class HighflyWorldSceneDefinition
    {
        public string id;
        public string sceneName;
        public string kind;
        public string[] zoneIds = new string[0];
        public string streamingPolicy;
        public string savePolicy;
        public HighflyWorldProvenance provenance = new HighflyWorldProvenance();
    }

    [Serializable]
    public sealed class HighflyZoneDefinition
    {
        public string id;
        public string sceneRef;
        public string displayName;
        public string[] biomeTags = new string[0];
        public string[] entryPoints = new string[0];
        public string[] exits = new string[0];
        public string[] serviceRefs = new string[0];
        public string[] portalRefs = new string[0];
        public string savePolicy;
        public string streamingPolicy;
        public HighflyWorldProvenance provenance = new HighflyWorldProvenance();
    }

    [Serializable]
    public sealed class HighflyInteriorDefinition
    {
        public string id;
        public string displayName;
        public string sceneName;
        public string layoutProfile;
        public string entryAnchorId;
        public string exitAnchorId;
        public string serviceRef;
        public string returnAnchorId;
        public string savePolicy;
        public HighflyWorldProvenance provenance = new HighflyWorldProvenance();
    }

    [Serializable]
    public sealed class HighflyPortalDefinition
    {
        public string id;
        public string displayName;
        public string sourceZoneId;
        public string destinationRef;
        public string returnAnchorId;
        public string accessPolicy;
        public HighflyWorldProvenance provenance = new HighflyWorldProvenance();
    }

    [Serializable]
    public sealed class HighflyWorldReturnState
    {
        public bool valid;
        public string exteriorSceneName;
        public string exteriorAnchorId;
        public Vector3 position;
        public Quaternion rotation;
        public string worldPhase;

        public void Clear()
        {
            valid = false;
            exteriorSceneName = "";
            exteriorAnchorId = "";
            position = Vector3.zero;
            rotation = Quaternion.identity;
            worldPhase = "";
        }
    }

    public enum HighflyWorldTransitionKind
    {
        EnterInterior,
        ExitInterior,
        Portal,
        Region,
        Dungeon,
        UniqueScenario
    }

    public sealed class HighflyWorldTransitionEvent
    {
        public readonly HighflyWorldTransitionKind Kind;
        public readonly string FromRef;
        public readonly string ToRef;
        public readonly string AnchorId;

        public HighflyWorldTransitionEvent(
            HighflyWorldTransitionKind kind,
            string fromRef,
            string toRef,
            string anchorId)
        {
            Kind = kind;
            FromRef = fromRef ?? "";
            ToRef = toRef ?? "";
            AnchorId = anchorId ?? "";
        }
    }

    // Typed boundary requested by COMBAT v2.1. These are contracts only:
    // Combat may emit requests; World remains the only authority that resolves them.
    public sealed class HighflyTeleportRequest
    {
        public string destinationRef;
        public string returnAnchorId;
        public string reason;
    }

    public sealed class HighflyInteractionRequest
    {
        public string interactionId;
        public string actorId;
        public string reason;
    }

    public sealed class HighflyQuestSignal
    {
        public string signalId;
        public string sourceId;
        public string payload;
    }

    public sealed class HighflySpawnRequest
    {
        public string profileId;
        public string ownerId;
        public string lifetimePolicy;
    }
}

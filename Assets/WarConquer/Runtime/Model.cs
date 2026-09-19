using System;
using System.Collections.Generic;
using System.Linq;

namespace WarConquer
{
    public enum Biome { Neutral, Forest, Swamp, Tundra, Volcanic, Desert, AshLand, Wasteland }
    public enum Category { Unit, Structure, Spell, Latent }
    public enum Phase { Setup, Start, Actions, End, Finished }
    [Serializable] public class EffectData
    {
        public string trigger, operation, target, biome;
        public int amount = 1, count = 1, duration = 1;
    }
    [Serializable] public class CardData
    {
        public string id, name, leader, factionTag, movementType, description;
        public Category category;
        public string[] subtypes, terrainTags, traits;
        public Biome[] biomes;
        public EffectData[] effects;
        public int energyCost, health, attack, movement, range, quantity, maxCopies = 3;
        public bool requiresAshLand;
        public bool IsStructure => category == Category.Structure || (category == Category.Latent && movementType == "Fixed");
        public bool Has(string trait) => traits != null && traits.Contains(trait);
        public bool Tag(string tag) => terrainTags != null && terrainTags.Contains(tag);
    }
    [Serializable] public class CardInstance { public int instanceId; public string cardId; }
    [Serializable] public class Piece
    {
        public int id, owner, health, tileId, remainingMovement, bonusAttack, permanentAttack;
        public string cardId;
        public bool token, attacked, abilityUsed, moved, evolved, fastBonusUsed;
        public int poison, poisonTurns, sleepUntilTurn, slow, slowUntilTurn, protectionRound = -1, forestSinceTurn = -1;
    }
    [Serializable] public class ResourcePool
    {
        public Biome biome;
        public string requiredTag;
        public int amount;
    }
    [Serializable] public class TerrainEffect
    {
        public int threshold = 4, damage = 1, expiresTurn = -1, expiresRound = -1, sourceId = -1, bonusDamage, bonusGroup;
        public bool onExit;
        public string compatibleTag = "DESIERTO";
    }
    [Serializable] public class HexTile
    {
        public int id, territory, owner = -1, baseOwner = -1, q, r, resource;
        public float x, y;
        public Biome biome;
        public bool blocked, hiddenAsh, permanentAsh, hiddenResource;
        public Piece unit, structure;
        public TerrainEffect specialEffect;
        public List<int> neighbors = new List<int>();
        public bool IsOccupied => unit != null || structure != null;
        public bool BlocksMovement => blocked || (structure != null && structure.cardId!="arena-profunda" && structure.cardId!="dunas-movedizas" && structure.cardId!="oasis-de-cristal");
    }
    [Serializable] public class Connection { public int a, b; public bool bridge; }
    [Serializable] public class FastRoute
    {
        public int a, b, owner, sourceId;
        public bool protectedRoute;
    }
    [Serializable] public class Player
    {
        public int id, leaderHealth, currentEnergy, maxEnergy, turnsTaken, spores, centerScore;
        public string leader, factionTag;
        public bool eliminated, terraformDiscountUsed, towerUsed;
        public int structureDiscount, dreamRound = -1, freeSteps;
        public List<CardInstance> deck = new List<CardInstance>(), hand = new List<CardInstance>(), discardPile = new List<CardInstance>();
        public List<ResourcePool> resources = new List<ResourcePool>();
    }
    [Serializable] public class Rules
    {
        public int initialHand = 5, drawPerTurn = 1, initialEnergy = 3, energyGrowth = 1, maxEnergy = 10;
        public int leaderHealth = 25, terraformCost = 1, revealAshCost = 3, leaderAbilityCost = 3;
        public int poisonDuration = 2, spellRange = 3, baseDeploymentRadius = 2, centerVictoryScore = 0;
        public int evolvedHealth = 6, evolvedAttack = 3, evolvedMovement = 3, maxResourcesPerBiome = 20;
        public bool drawOnFirstTurn, summoningSickness, allowNeutralDeployment = true, ashOnMarkedDestruction = true;
    }
    [Serializable] public class GameState
    {
        public int version = 1, seed, randomState, activePlayer, turn = 1, round = 1, nextId = 1, winner = -1;
        public Phase phase;
        public Rules rules;
        public List<Player> players = new List<Player>();
        public List<HexTile> tiles = new List<HexTile>();
        public List<Connection> connections = new List<Connection>();
        public List<FastRoute> fastRoutes = new List<FastRoute>();
        public List<string> log = new List<string>();
        public Player Active => players[activePlayer];
        public int Random(int max)
        {
            uint x = (uint)randomState; x ^= x << 13; x ^= x >> 17; x ^= x << 5;
            randomState = (int)x; return (int)(x % max);
        }
        public void Log(string message) { log.Add(message); if (log.Count > 100) log.RemoveAt(0); }
    }
    public static class Names
    {
        public static readonly string[] Biomes = { "Neutro", "Bosque", "Pantano", "Tundra", "Volcánico", "Desierto", "Tierra Ceniza", "Yermo" };
        public static readonly string[] Categories = { "Unidad", "Estructura", "Magia", "Latente" };
        public static string BiomeTag(Biome b) => new[] { "", "BOSQUE", "PANTANO", "TUNDRA", "VOLCANICO", "DESIERTO", "CENIZA", "YERMO" }[(int)b];
    }
}

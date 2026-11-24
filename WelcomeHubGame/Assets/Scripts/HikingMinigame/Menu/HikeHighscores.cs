using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HikingMinigame.Menu
{
    public sealed class HikeHighscores
    {
        private const string TesterNamePrefix = "HikingTester_";
        private const string UnknownPlayerName = "Unknown hiker";
        private const int UserLookupLimit = 1000;

        private readonly int _minigameId;
        private Task<string> _testerRegistration;

        public HikeHighscores(int minigameId)
        {
            _minigameId = minigameId;
        }

        public Task<string> EnsurePlayerAsync()
        {
            if (HikeSession.HasPlayer)
                return Task.FromResult(HikeSession.PlayerGuid);

            return _testerRegistration ??= RegisterTesterAsync();
        }

        private static async Task<string> RegisterTesterAsync()
        {
            var tester = await MinigameApiClient.RegisterGuestAsync($"{TesterNamePrefix}{Random.Range(1000, 10000)}");
            if (tester != null)
                HikeSession.UsePlayer(tester.guid);

            return HikeSession.PlayerGuid;
        }

        public async Task<bool> SubmitAsync(HikeScore score, DateTime startedAt, DateTime endedAt)
        {
            var playerGuid = await EnsurePlayerAsync();
            if (playerGuid == null)
                return false;

            return await MinigameApiClient.SubmitScoreAsync(playerGuid, score.Total, _minigameId, startedAt, endedAt);
        }

        public async Task<IReadOnlyList<HighscoreEntry>> GetTopAsync(int count)
        {
            var scoresJson = await MinigameApiClient.GetHighscoresAsync(_minigameId, count);
            var usersJson = await MinigameApiClient.GetUsersAsync(UserLookupLimit);
            if (scoresJson == null || usersJson == null)
                return null;

            var namesByGuid = JsonUtility.FromJson<UserListJson>(usersJson).data
                .GroupBy(user => user.guid)
                .ToDictionary(group => group.Key, group => group.First().username);

            return JsonUtility.FromJson<ScoreListJson>(scoresJson).data
                .Select(entry => new HighscoreEntry(
                    namesByGuid.GetValueOrDefault(entry.user_id, UnknownPlayerName),
                    entry.score,
                    entry.user_id == HikeSession.PlayerGuid))
                .ToList();
        }
    }
}

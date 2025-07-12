const ScoreboardComponent = {
    props: ['gameData'],
    template: `<div class="scoreboard">
        <h2>Scoreboard</h2>
        <div v-if="gameData.teams">
            <div v-for="(team, key) in gameData.teams" :key="key">
                {{ key }}: {{ team.score }}
            </div>
        </div>
    </div>`
};

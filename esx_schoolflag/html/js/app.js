const { createApp } = Vue

const app = createApp({
    data() {
        return {
            gameState: {},
            showUI: false
        }
    }
})

app.component('scoreboard-component', ScoreboardComponent)
app.component('timer-component', TimerComponent)
app.component('player-list', PlayerListComponent)
app.component('game-status', GameStatusComponent)
app.component('next-game', NextGameComponent)

window.addEventListener('message', (event) => {
    const data = event.data
    if (data.action === 'show') {
        app.config.globalProperties.gameState = data.data
        app.config.globalProperties.showUI = true
    } else if (data.action === 'update') {
        app.config.globalProperties.gameState = data.data
    } else if (data.action === 'hide') {
        app.config.globalProperties.showUI = false
    }
})

app.mount('#app')

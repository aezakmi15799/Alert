const PlayerListComponent = {
    props: ['players'],
    template: `<ul class="players">
        <li v-for="(p, id) in players" :key="id">{{ p.identifier }} - {{ p.score }}</li>
    </ul>`
};

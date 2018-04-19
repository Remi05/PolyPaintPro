export class AchievementDefinitionModel {
    constructor(
        public name: string, // Name of the achievement
        public message: string, // Message when the achievement is unlocked
        public description: string, // Description of to unlock this achievement
        public metric: string,
        public iconUri: string,
        public count: number
    ) { }
}
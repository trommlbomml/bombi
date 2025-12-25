import Phaser from "phaser"
import BootScene from "./boot-scene"

const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  parent: "game",
  width: 800,
  height: 600,
  backgroundColor: "#1d1d1d",
  scene: [BootScene]
}

export default new Phaser.Game(config)

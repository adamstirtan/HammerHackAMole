<script setup>
  import { ref, onMounted } from 'vue';
  import * as signalR from '@microsoft/signalr';

  const gameSpeed = ref(750);
  const score = ref(0);
  const moles = ref(Array(16).fill(false));
  const soundEnabled = ref(false);
  const whackFileName = '/whack.mp3';
  const timeLeft = ref(10);
  const timerInterval = ref(null);
  const moleInterval = ref(null);
  const gameStarted = ref(false);
  const showRestartButton = ref(true);
  const baseUrl = import.meta.env.VITE_SIGNALR_URL;
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(baseUrl)
    .build();

  onMounted(async () => {
    try {
      await connection.start();

      connection.on('UpdateScore', (newScore) => {
        score.value = newScore;
      });
    } catch (error) {
      console.error(error);
    }
  });

  const startGame = () => {
    gameStarted.value = true;
    timeLeft.value = 10;
    moles.value = Array(16).fill(false);

    clearInterval(timerInterval.value);
    timerInterval.value = setInterval(() => {
      if (timeLeft.value > 0) {
        timeLeft.value--;
      } else {
        clearInterval(timerInterval.value);
        gameStarted.value = false;
        showRestartButton.value = true;
      }
    }, 1000);

    clearInterval(moleInterval.value);
    moleInterval.value = setInterval(() => {
      const randomIndex = Math.floor(Math.random() * moles.value.length);
      moles.value = moles.value.map((_, index) => index === randomIndex);
    }, gameSpeed.value);
  };

  const whackMole = async (index) => {
    if (moles.value[index]) {
      try {
        await connection.invoke('WhackMole');
        moles.value[index] = false;

        if (soundEnabled.value) {
          const whackSound = new Audio(whackFileName);
        }
      } catch (error) {
        console.error(error);
      }
    }
  };

  const restartGame = () => {
    startGame();
    showRestartButton.value = false;
  };
</script>

<template>
  <div class="container">
    <div v-if="gameStarted">
      <div class="score">
        <span v-if="score">{{ score }} Moles Hacked 🔨</span>
      </div>
      <div id="game">
        <div v-for="(mole, index) in moles" :key="index" class="hole" @click="whackMole(index)" v-if="!showRestartButton.value">
          <div v-if="mole" class="mole"></div>
        </div>
      </div>
      <progress max="10" :value="timeLeft"></progress>
      <div class="join">
        <span class="subtle">https://</span>tinyurl.com/36kjp7z4
      </div>
    </div>
    <button v-if="showRestartButton" @click="restartGame">Start Game</button>
  </div>
</template>


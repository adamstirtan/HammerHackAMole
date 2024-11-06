<script setup>
  import { ref, onMounted } from 'vue';
  import * as signalR from '@microsoft/signalr';

  const score = ref(0);
  const moles = ref(Array(9).fill(false));
  const whackSound = new Audio('whack.mp3');
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

      startGame();
    } catch (error) {
      console.error(error);
    }
  });

  const startGame = () => {
    setInterval(() => {
      const randomIndex = Math.floor(Math.random() * moles.value.length);
      moles.value = moles.value.map((_, index) => index === randomIndex);
    }, 1000);
  };

  const whackMole = async (index) => {
    if (moles.value[index]) {
      try {
        await connection.invoke('WhackMole');
        moles.value[index] = false;
        whackSound.play();
      } catch (error) {
        console.error(error);
      }
    }
  };
</script>

<template>
  <div class="container">
    <div class="score">Score: {{ score }}</div>
    <div id="game">
      <div v-for="(mole, index) in moles" :key="index" class="hole" @click="whackMole(index)">
        <div v-if="mole" class="mole"></div>
      </div>
    </div>
    <div class="join">
      <span class="subtle">https://</span>tinyurl.com/36kjp7z4
    </div>
  </div>
</template>


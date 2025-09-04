<template>
  <div :class="`inline-flex items-center gap-1 ${className}`">
    <!-- N Block -->
    <div 
      class="bg-white flex items-center justify-center text-neo-yellow font-bold"
      :style="{
        height: `${blockHeight}px`,
        width: `${nWidth}px`,
        fontSize: `${fontSize}px`
      }"
    >
      N
    </div>
    
    <!-- E Block -->
    <div 
      class="bg-white flex items-center justify-center text-neo-yellow font-bold"
      :style="{
        height: `${blockHeight}px`,
        width: `${eWidth}px`,
        fontSize: `${fontSize}px`
      }"
    >
      E
    </div>
    
    <!-- O Block -->
    <div 
      class="bg-neo-yellow relative flex items-center justify-center"
      :style="{
        height: `${blockHeight}px`,
        width: `${oWidth}px`
      }"
    >
      <!-- Complete O letter in white -->
      <div 
        class="text-white font-bold"
        :style="{ fontSize: `${fontSize}px` }"
      >
        O
      </div>
    </div>
    
    <!-- Ledge Text -->
    <span 
      class="text-neo-dark-gray font-normal ml-2"
      :style="{ fontSize: `${fontSize}px` }"
    >
      <span class="uppercase">L</span>edge
    </span>
  </div>
</template>

<script setup>
import { computed } from 'vue'

// Define props
const props = defineProps({
  className: {
    type: String,
    default: ''
  },
  height: {
    type: Number,
    default: 40
  }
})

// Computed properties for dimensions
const blockHeight = computed(() => props.height)
const nWidth = computed(() => props.height * 0.8)
const eWidth = computed(() => props.height * 0.6) // Narrower than N square
const oWidth = computed(() => props.height * 0.8)
const fontSize = computed(() => props.height * 0.7)
</script>

<style scoped>
/* Define custom colors if not using Tailwind's custom config */
.text-neo-yellow {
  color: #F6BF00;
}

.bg-neo-yellow {
  background-color: #F6BF00;
}

.text-neo-dark-gray {
  color: #1D1D1D;
}

/* Utility classes if not using Tailwind */
.inline-flex {
  display: inline-flex;
}

.items-center {
  align-items: center;
}

.justify-center {
  justify-content: center;
}

.gap-1 {
  gap: 0.25rem;
}

.bg-white {
  background-color: white;
}

.text-white {
  color: white;
}

.font-bold {
  font-weight: bold;
}

.font-normal {
  font-weight: normal;
}

.ml-2 {
  margin-left: 0.5rem;
}

.uppercase {
  text-transform: uppercase;
}

.relative {
  position: relative;
}

.flex {
  display: flex;
}
</style>
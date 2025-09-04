<template>
  <div>
    <h1>Hello Admin</h1>
    <button @click="logout">Logout</button>
  </div>
</template>

<script setup>
import { router } from '../../routers/index.js'

const logout = () => {
  router.logout()
}

// Hardcode "Admin" for now
const userRole = 'Admin'
</script>     
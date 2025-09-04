<template>
  <div class="form-group">
    <label v-if="label" :for="id" class="form-label">
      {{ label }}
      <span v-if="required" class="required">*</span>
    </label>
    <div class="input-wrapper" :class="{ 'has-error': error, 'is-focused': isFocused }">
      <input
        :id="id"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :required="required"
        class="form-input"
        @input="$emit('update:modelValue', $event.target.value)"
        @focus="isFocused = true"
        @blur="isFocused = false"
      />
    </div>
    <span v-if="error" class="error-message">{{ error }}</span>
  </div>
</template>

<script setup>
import { ref } from 'vue'

defineProps({
  id: String,
  type: {
    type: String,
    default: 'text'
  },
  label: String,
  placeholder: String,
  modelValue: String,
  required: Boolean,
  error: String
})

defineEmits(['update:modelValue'])

const isFocused = ref(false)
</script>

<style scoped>
.form-group {
  margin-bottom: 1.5rem;
}

.form-label {
  display: block;
  color: #1D1D1D;
  font-size: 0.875rem;
  font-weight: 600;
  margin-bottom: 0.5rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.required {
  color: #ef4444;
}

.input-wrapper {
  position: relative;
  transition: all 0.2s ease;
}

.form-input {
  width: 100%;
  padding: 0.875rem 1rem;
  border: 2px solid #EEEEEE;
  border-radius: 12px;
  font-size: 1rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  color: #1D1D1D;
  background: white;
  transition: all 0.2s ease;
  box-sizing: border-box;
}

.form-input:focus {
  outline: none;
  border-color: #F6BF00;
  box-shadow: 0 0 0 3px rgba(246, 191, 0, 0.1);
}

.form-input::placeholder {
  color: #9CA3AF;
}

.is-focused .form-input {
  border-color: #F6BF00;
  box-shadow: 0 0 0 3px rgba(246, 191, 0, 0.1);
}

.has-error .form-input {
  border-color: #ef4444;
}

.has-error.is-focused .form-input {
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.error-message {
  display: block;
  color: #ef4444;
  font-size: 0.75rem;
  margin-top: 0.375rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}
</style>
<script lang="ts">
  import type { UserDto } from "../api";
  import { getInitials } from "../utils";

  export let user: UserDto;
</script>

<div class="avatar" title={user.name}>
  <div class="initials">
    {#if user.photo}
      <img class="avatar" src={user.photo} alt={`Profile photo of ${user.name}`} />
    {:else}
      {getInitials(user.name)}
    {/if}
  </div>
</div>

<style>
  .avatar {
    display: flex;
    font-size: 1rem;
    background-color: #ff4b33;
    width: 2rem;
    height: 2rem;
    border-radius: 1rem;
    justify-content: center;
    align-items: center;
    color: white;
  }
</style>

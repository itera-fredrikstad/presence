<script lang="ts">
  import { beforeUpdate } from "svelte";
  import { format, isFirstDayOfMonth, isSaturday, isSunday, isSameDay, isMonday, getISOWeek } from "date-fns";
  import { nb } from "date-fns/locale";

  import type { Identifiable, DayAtWork, DayAtWorkType } from "../models";
  import type { PublicHoliday, TeamEvent } from "../api";
  import { emphasizeEmojis } from "../utils";
  import { tooltip } from "./tooltip";

  export let day: Date;
  export let dayAtWork: Identifiable<DayAtWork> | undefined = null;
  export let publicHoliday: PublicHoliday | undefined = null;
  export let teamEvents: TeamEvent[] | undefined = [];
  export let onUpdate: (dayAtWork: Partial<DayAtWork>) => void;

  export let showDayName: boolean = true;
  export let showWeekNumber: boolean = false;
  export let isActive: boolean = true;

  let editComment: boolean;
  let commentField: any;
  let comment: string;

  let oldDayAtWork: Identifiable<DayAtWork>;

  beforeUpdate(() => {
    if (oldDayAtWork !== dayAtWork) {
      comment = dayAtWork?.comment;
      oldDayAtWork = dayAtWork;
    }
  });

  function isSelected(dayAtWork: Identifiable<DayAtWork>): boolean {
    return !!dayAtWork && !!dayAtWork.type;
  }

  function isFull(dayAtWork: Identifiable<DayAtWork>): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "FULL";
  }

  function isFirstHalf(dayAtWork: Identifiable<DayAtWork>): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "FIRST-HALF";
  }

  function isLastHalf(dayAtWork: Identifiable<DayAtWork>): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "LAST-HALF";
  }

  function isEmpty(dayAtWork: Identifiable<DayAtWork>): boolean {
    return isSelected(dayAtWork) && dayAtWork.type == "EMPTY";
  }

  function onClick() {
    if (isFirstHalf(dayAtWork)) {
      onUpdate({ date: day, type: "LAST-HALF" });
      return;
    }

    if (isLastHalf(dayAtWork)) {
      onUpdate({ date: day, type: "EMPTY" });
      return;
    }

    if (isFull(dayAtWork)) {
      onUpdate({ date: day, type: "FIRST-HALF" });
      return;
    }

    onUpdate({ type: "FULL" });
  }

  function handleUpdateComment() {
    if (!editComment) {
      return;
    }

    editComment = false;

    onUpdate({ comment });
  }

  function handleStartEditComment(e) {
    e.stopPropagation();
    editComment = true;
    setTimeout(() => {
      commentField.setSelectionRange(0, commentField.value.length);
      commentField.focus();
    }, 100);
  }

  function isNonWorkingDay(day: Date) {
    return isSaturday(day) || isSunday(day);
  }

  const beerTriggers = ["pils", "fest", "øl"];

  function isBeerEvent(eventName: string) {
    return beerTriggers.filter((t) => eventName.toLowerCase().includes(t)).length > 0;
  }
</script>

<div>
  <div
    class:selected={isSelected(dayAtWork) && !isEmpty(dayAtWork)}
    class:first-half={isFirstHalf(dayAtWork)}
    class:last-half={isLastHalf(dayAtWork)}
    class="day"
    class:non-working={isNonWorkingDay(day) || !!publicHoliday}
    class:inactive={!isActive}
    on:click={onClick}
  >
    <i
      class="fa-solid fa-comment comment-icon"
      use:tooltip
      data-tooltip="Legg inn/redigér kommentar"
      on:click={handleStartEditComment}
    />

    <h1>
      {format(day, "d", { locale: nb })}
      {#if isFirstDayOfMonth(day)}
        {format(day, "MMMM", { locale: nb })}
      {/if}
    </h1>
    {#if showDayName}
      <h2>{format(day, "eeee", { locale: nb })}</h2>
    {/if}
    {#if showWeekNumber && isMonday(day)}
      <h3>uke {getISOWeek(day)}</h3>
    {/if}
    {#if !!publicHoliday}
      <h3>{publicHoliday.name}</h3>
    {/if}
    {#each teamEvents as teamEvent}
      {@const eventName = isBeerEvent(teamEvent.name) ? "🍻 " + teamEvent.name : teamEvent.name}
      <h3>
        {@html emphasizeEmojis(eventName)} ({format(teamEvent.start, "HH:mm")}-{format(
          teamEvent.end,
          "HH:mm"
        )})
      </h3>
    {/each}
    {#if editComment}
      <textarea
        bind:this={commentField}
        bind:value={comment}
        class="inactive"
        class:hide={!comment && !editComment}
        on:blur={handleUpdateComment}
        on:click|stopPropagation
      />
    {:else if comment}
      <p class="comment">
        {@html emphasizeEmojis(comment || "")}
      </p>
    {/if}
  </div>
</div>

<style>
  .day {
    aspect-ratio: 1;
    background-color: #efefef;
    padding: 1rem;
    border: 1px solid #999;
    display: flex;
    flex-direction: column;
    width: 100%;
    height: 100%;
    position: relative;
  }

  :global(.dark) .day {
    background-color: #333;
    border: 1px solid #444;
  }

  @media only screen and (max-width: 480px) {
    .day {
      margin-bottom: 1rem;
    }
  }

  .comment-icon {
    position: absolute;
    right: 1rem;
    top: 1rem;
    font-size: 1.5rem;
    cursor: pointer;
    color: #666;
  }

  :global(.dark) .comment-icon {
    color: #999;
  }

  .non-working {
    color: #999;
    background: repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #dfdfdf 5px, #dfdfdf 10px);
  }

  .inactive {
    opacity: 40%;
  }

  :global(.dark) .non-working {
    background: repeating-linear-gradient(-45deg, #444, #444 5px, #666 5px, #666 10px);
  }

  .day:hover {
    background-color: #eaeaea;
  }
  :global(.dark) .day:hover {
    background-color: #444;
  }
  .day textarea:focus {
    outline: none;
  }

  .day textarea.inactive {
    user-select: none;
  }

  .day textarea.hide {
    display: none;
  }

  .day.selected {
    background: repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  :global(.dark) .day.selected {
    background: repeating-linear-gradient(-45deg, #666, #666 5px, #ff4b33 5px, #ff4b33 10px);
  }

  .day.selected.first-half {
    background: linear-gradient(
        to top,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  :global(.dark) .day.selected.first-half {
    background: linear-gradient(
        to top,
        #666 0%,
        #666 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #666, #666 5px, #ff4b33 5px, #ff4b33 10px);
  }

  .day.selected.last-half {
    background: linear-gradient(
        to bottom,
        rgba(238, 238, 238, 1) 0%,
        rgba(238, 238, 238, 1) 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #efefef, #efefef 5px, #ffcccb 5px, #ffcccb 10px);
  }

  :global(.dark) .day.selected.last-half {
    background: linear-gradient(
        to bottom,
        #666 0%,
        #666 50%,
        rgba(238, 238, 238, 0) 50%,
        rgba(238, 238, 238, 0) 100%
      ),
      repeating-linear-gradient(-45deg, #666, #666 5px, #ff4b33 5px, #ff4b33 10px);
  }

  .day h1 {
    margin: 0;
    font-size: 1rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
    width: fit-content;
  }

  .day h2 {
    margin: 0;
    font-size: 1.2rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
    width: fit-content;
  }

  .day h3 {
    margin: 0;
    font-size: 0.6rem;
    background-color: #efefef;
    padding: 0.2rem 0.5rem;
    text-transform: uppercase;
    width: fit-content;
  }

  :global(.dark) .day h1,
  :global(.dark) .day h2,
  :global(.dark) .day h3 {
    background-color: #555;
  }

  h3 > :global(.emoji) {
    font-size: 1rem;
  }

  .day textarea {
    border: none;
    background: #efefef;
    padding: 0.2rem 0.5rem;
    width: 100%;
    margin: 0;
    font-size: 0.8rem;
    resize: none;
    overflow: hidden;
    cursor: default;
    flex-grow: 100;
  }

  .comment {
    background: #efefef;
    padding: 0.2rem 0.5rem;
    margin: 0;
    font-size: 0.8rem;
    resize: none;
    overflow: hidden;
    cursor: default;
    width: fit-content;
    font-weight: 300;
    white-space: pre-wrap;
  }

  :global(.dark) .comment {
    background-color: #555;
  }

  .comment > :global(.emoji) {
    font-size: 1.2rem;
  }
</style>
